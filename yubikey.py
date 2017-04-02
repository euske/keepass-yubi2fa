#!/usr/bin/env python
import sys
import time
from struct import unpack
from Crypto.Cipher import AES

MOD2HEX = dict(
    (ord(c1),ord(c2)) for (c1,c2) in
    zip('cbdefghijklnrtuv', '0123456789abcdef') )
def modhex(s):
    s = s.translate(MOD2HEX)
    return bytes.fromhex(s)

def getcrc(data):
    v = 0xffff
    for b in data:
        v ^= b
        for _ in range(8):
            n = v & 1
            v >>= 1
            if n != 0:
                v ^= 0x8408
    return v

pid=modhex('vvrginnubkjj')
vid=bytes.fromhex('a09aa8bf3dd1')
key=bytes.fromhex('e2a2e4c88067a1e2efe238f8d2d3f76c')
# python yubikey.py vvrginnubkjjgjrbkrifkikcrunlkjdlbfkcdberjubn

def main(argv):
    v = argv[1]
    b = modhex(v)
    kid = b[:6]
    assert kid == pid
    kval = b[6:]
    c = AES.new(key, AES.MODE_ECB)
    code = c.decrypt(kval)
    print (code.hex())
    assert code[:6] == vid
    assert getcrc(code) == 0xf0b8
    (uc,ts1,ts0,sc) = unpack('<HHBB4x', code[6:])
    ts = (ts0 << 16 | ts1)
    print (uc,sc,ts)
    return 0

if __name__ == '__main__': sys.exit(main(sys.argv))
