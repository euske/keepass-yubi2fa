// Yubi2FA.cs

using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;

using KeePass.Plugins;
using KeePassLib.Keys;

[assembly: AssemblyTitle("Yubi2FA")]
[assembly: AssemblyDescription("Key provider for YubiKey 2-factor authentication.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Yusuke Shinyama")]
[assembly: AssemblyProduct("KeePass Plugin")]
[assembly: AssemblyCopyright("Copyright (c) 2017 Yusuke Shinyama")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace Yubi2FA {

public sealed class Yubi2FAExt : Plugin {
    
    private Yubi2FAKeyProv _prov;
    private IPluginHost _host;
    
    public override bool Initialize(IPluginHost host) {
	_prov = new Yubi2FAKeyProv();
	_host = host;
	_host.KeyProviderPool.Add(_prov);
	return true;
    }
    
    public override void Terminate() {
	_host.KeyProviderPool.Remove(_prov);
    }
}

public sealed class Yubi2FAKeyProv : KeyProvider {

    public override string Name {
	get { return "YubiKey 2FA Key Provider"; }
    }

    public override bool SecureDesktopCompatible {
        get { return true; }
    }
    
    public string StateFileName {
        get { return "yubi2fa.sav"; }
    }

    public override byte[] GetKey(KeyProviderQueryContext ctx) {
        string dirname = Path.GetDirectoryName(ctx.DatabaseIOInfo.Path);
        string statePath = Path.Combine(dirname, StateFileName);
        if (ctx.CreatingNewKey) {
            return promptForNewKey(statePath);
        } else {
            return promptForOTP(statePath);
        }
    }

    private byte[] promptForNewKey(string statePath) {
        using (Yubi2FAConfigForm form = new Yubi2FAConfigForm()) {
            if (form.ShowDialog() == DialogResult.OK &&
                form.ConfigLine != null) {
                try {
                    YubiKeyOTPItem item = YubiKeyOTPItem.FromLog(form.ConfigLine);
                    writeEntry(statePath, item.ToEntry());
                    return item.PrivId;
                } catch (FormatException ) {
                    // ERROR: Invalid configuration format.
                    showError(form, "Invalid configuration format.");
                } catch (IOException e) {
                    // ERROR: Could not write the state file.
                    showError(form, "Could not write the state file.\n\n"+e);
                }
            }
        }
        return null;
    }
    
    private byte[] promptForOTP(string statePath) {
        using (Yubi2FAInputForm form = new Yubi2FAInputForm()) {
            try {
                string entry = readEntry(statePath);
                if (form.ShowDialog() == DialogResult.OK &&
                    form.OTP != null) {
                    try {
                        YubiKeyOTPItem item = YubiKeyOTPItem.FromEntry(entry);
                        if (item.Verify(form.OTP)) {
                            try {
                                writeEntry(statePath, item.ToEntry());
                                return item.PrivId;
                            } catch (IOException e) {
                                // ERROR: Could not update the state file.
                                showError(form, "Could not update the state file.\n\n"+e);
                            }
                        } else {
                            // ERROR: Could not verify the OTP.
                            showError(form, "Could not verify the OTP.");
                        }
                    } catch (FormatException ) {
                        // ERROR: Invalid OTP.
                        showError(form, "Invalid OTP format.");
                    }
                } else if (form.ConfigLine != null) {
                    try {
                        YubiKeyOTPItem item = YubiKeyOTPItem.FromLog(form.ConfigLine);
                        writeEntry(statePath, item.ToEntry());
                        return item.PrivId;
                    } catch (FormatException ) {
                        // ERROR: Invalid configuration format.
                        showError(form, "Invalid configuration format.");
                    } catch (IOException e) {
                        // ERROR: Could not write the state file.
                        showError(form, "Could not write the state file.\n\n"+e);
                    }
                }
            } catch (FormatException ) {
                // ERROR: Invalid state file format.
                showError(form, "Invalid state file format.");
            } catch (IOException e) {
                // ERROR: Could not read the state file.
                showError(form, "Could not read the state file.\n\n"+e);
            }
        }
        return null;
    }

    private void showError(Form parent, string message) {
        MessageBox.Show(
            parent, message, "Yubi2FA Error",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void writeEntry(string path, string entry) {
        using (StreamWriter fp = new StreamWriter(path)) {
            fp.WriteLine(entry);
        }
    }
    
    private string readEntry(string path) {
        using (StreamReader fp = new StreamReader(path)) {
            return fp.ReadLine();
        }
    }
}
    
} // Yubi2FA
