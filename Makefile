# Makefile

DEL=del /f
COPY=copy /y
CSC=csc /nologo
RC=rc /nologo

KEEPASS_DIR="%ProgramFiles(x86)%\KeePass Password Safe 2"
KEEPASS_APP=$(KEEPASS_DIR)\KeePass.exe
KEEPASS_PLUGINS=$(KEEPASS_DIR)\Plugins
KEEPASS_TESTDB=.\tmp\TestDatabase.kdbx

CSCFLAGS=/warn:4 /optimize+ /debug:pdbonly
REFERENCES=/r:lib\keepass.exe

all: Yubi2FA.dll

clean:
	-$(DEL) *.exe *.dll *.pdb

run: install
	$(KEEPASS_APP) $(KEEPASS_TESTDB)

install: Yubi2FA.dll
	$(COPY) $** $(KEEPASS_PLUGINS)

Yubi2FA.dll: Yubi2FA.cs YubiKeyOTP.cs Yubi2FAForms.cs
	$(CSC) $(CSCFLAGS) /out:$@ /t:library $(REFERENCES) $**

.cs.obj:
	$(CC) $(CFLAGS) /Fo$@ /c $< $(DEFS) $(INCLUDES)
.rc.res:
	$(RC) $(RCFLAGS) $<
