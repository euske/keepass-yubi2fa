// YubiKeyOTP.cs

using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Yubi2FA {

public class YubiKeyOTPItem {
    
    public byte[] PubId;
    public byte[] AESKey;
    public byte[] PrivId;
    public uint UCount;
    public uint SCount;

    public YubiKeyOTPItem(byte[] PubId, byte[] AESKey) {
        this.PubId = PubId;
        this.AESKey = AESKey;
        this.PrivId = null;
        this.UCount = 0;
        this.SCount = 0;
    }

    public override string ToString() {
        return string.Format(
            "<YubiKeyOTPItem: PubId={0}, AESKey={1}, UCount={2}, SCount={3}>",
            YubiKeyOTPUtils.ToHex(PubId), YubiKeyOTPUtils.ToHex(AESKey),
            UCount, SCount);
    }

    public string ToEntry() {
        return string.Format(
            "{0},{1},{2},{3}",
            YubiKeyOTPUtils.ToModHex(PubId), YubiKeyOTPUtils.ToHex(AESKey),
            UCount, SCount);
    }

    public bool Verify(string otpText) {
        return Verify(YubiKeyOTPUtils.ParseModHex(otpText));
    }
    
    public bool Verify(byte[] otp) {
	if (otp.Length != 22) {
	    throw new FormatException("Invalid OTP length");
	}
	if (!YubiKeyOTPUtils.BytesEqual(otp, 0, PubId, 0, PubId.Length)) {
            // Id not matched.
	    return false;
	}
	byte[] code = new byte[16];
	Buffer.BlockCopy(otp, 6, code, 0, code.Length);
	byte[] data = YubiKeyOTPUtils.DecryptBytesAes(code, AESKey);
	if (YubiKeyOTPUtils.GetYubiKeyCRC(data) != 0xf0b8) {
            // Invalid OTP.
	    return false;
	}
	uint uc = (uint)data[7] << 8 | (uint)data[6];
	uint sc = (uint)data[11];
	if (uc < UCount ||
	    (uc == UCount && sc <= SCount)) {
            // OTP Reused.
	    return false;
	}
	UCount = uc;
	SCount = sc;
	PrivId = new byte[6];
	Buffer.BlockCopy(data, 0, PrivId, 0, PrivId.Length);	
	return true;
    }

    public static YubiKeyOTPItem FromLog(string line) {
        string[] cols = YubiKeyOTPUtils.ParseCSVLine(line);
        if (cols.Length < 6 || cols[0] != "Yubico OTP") {
            throw new FormatException("Invalid format: "+line);
        }
        YubiKeyOTPItem item = new YubiKeyOTPItem(
            YubiKeyOTPUtils.ParseModHex(cols[3]),
            YubiKeyOTPUtils.ParseHex(cols[5]));
        item.PrivId = YubiKeyOTPUtils.ParseHex(cols[4]);
        return item;
    }
    
    public static YubiKeyOTPItem FromEntry(string line) {
        string[] cols = YubiKeyOTPUtils.ParseCSVLine(line);
        if (cols.Length < 4) {
            throw new FormatException("Invalid format: "+line);
        }
        YubiKeyOTPItem item = new YubiKeyOTPItem(
            YubiKeyOTPUtils.ParseModHex(cols[0]),
            YubiKeyOTPUtils.ParseHex(cols[1]));
        item.UCount = UInt32.Parse(cols[2]);
        item.SCount = UInt32.Parse(cols[3]);
        return item;
    }

    public static void CheckOTP(string otpText) {
        byte[] otp = YubiKeyOTPUtils.ParseModHex(otpText);
	if (otp.Length != 22) {
	    throw new FormatException("Invalid OTP length");
	}
    }
}

public class YubiKeyOTPUtils {

    public static byte[] DecryptBytesAes(byte[] cipher, byte[] key) {
	byte[] b = new byte[cipher.Length];
	using (AesManaged aes = new AesManaged()) {
	    aes.Mode = CipherMode.ECB;
	    aes.Padding = PaddingMode.None;
	    ICryptoTransform decr = aes.CreateDecryptor(key, null);
	    using (MemoryStream ms = new MemoryStream(cipher)) {
		using (CryptoStream cs = new CryptoStream(
			   ms, decr, CryptoStreamMode.Read)) {
		    if (cs.Read(b, 0, b.Length) == b.Length) {
			return b;
		    }
		}
	    }
	}
	throw new ApplicationException("AES Error");
    }

    public static string ToHex(byte[] b) {
	return BitConverter.ToString(b).Replace("-", "");
    }
    
    public static byte[] ParseHex(string h) {
        if (h.Length % 2 != 0) {
            throw new FormatException("Invalid hex length: "+h);
        }
	List<byte> b = new List<byte>();
	for (int i = 0; i < h.Length; i += 2) {
	    b.Add(Convert.ToByte(h.Substring(i, 2), 16));
	}
	return b.ToArray();
    }

    private const string MOD = "cbdefghijklnrtuv";
    private const string HEX = "0123456789abcdef";
    
    public static string ToModHex(byte[] b) {
	string s = ToHex(b);
	string m = "";
	foreach (char c in s) {
	    int i = HEX.IndexOf(char.ToLower(c));
	    m += MOD[i];
	}
	return m;
    }
    
    public static byte[] ParseModHex(string s) {
	string h = "";
	foreach (char c in s) {
	    int i = MOD.IndexOf(c);
	    if (i < 0) {
		throw new FormatException("Invalid character: "+c);
	    }
	    h += HEX[i];
	}
	return ParseHex(h);
    }
    
    public static string[] ParseCSVLine(string line) {
	List<string> cols = new List<string>();
	int i0 = 0;
	for (int i = 0; i < line.Length; i++) {
	    char c = line[i];
            if (c == '\n') {
                break;
            } else if (c == ',') {
		string value = line.Substring(i0, i - i0);
		cols.Add(value);
		i0 = i + 1;
	    }
	}
	cols.Add(line.Substring(i0));
	return cols.ToArray();
    }

    public static bool BytesEqual(byte[] b0, int i0, byte[] b1, int i1, int n) {
	for (int d = 0; d < n; d++) {
	    if (b0[i0+d] != b1[i1+d]) return false;
	}
	return true;
    }

    public static uint GetYubiKeyCRC(byte[] data) {
	uint v = 0xffff;
	foreach (byte b in data) {
	    v ^= b;
	    for (int i = 0; i < 8; i++) {
		uint n = v & 1;
		v >>= 1;
		if (n != 0) {
		    v ^= 0x8408;
		}
	    }
	}
	return v;
    }

    public static void Main() {
	YubiKeyOTPItem item = YubiKeyOTPItem.FromLog("Yubico OTP,2017-04-01 00:00,1,vvrginnubkjj,a09aa8bf3dd1,e2a2e4c88067a1e2efe238f8d2d3f76c,,,0,0,0,0,0,0,0,0,0,0");
	Console.WriteLine(item);
	byte[] otp = ParseModHex("vvrginnubkjjgjrbkrifkikcrunlkjdlbfkcdberjubn");
	if (item.Verify(otp)) {
	    Console.WriteLine(item);
	    Console.WriteLine(ToHex(item.PrivId));
	}
    }
}
    
} // Yubi2FA
