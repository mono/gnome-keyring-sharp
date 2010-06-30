SAMPLES=secret.cs keyring-showall.cs

EXTRA_DIST=$(SAMPLES)

.cs.exe:
	mcs -pkg:gnome-keyring-sharp-1.0 $^

samples: secret.exe keyring-showall.exe
	cp -uv ../src/Gnome.Keyring.dll* . || echo "ERROR: You need to run 'make install' in the toplevel directory"

secret.exe: secret.cs

keyring-showall.exe: keyring-showall.cs

clean:
	-rm -f secret.exe keyring-showall.exe Gnome.Keyring.dll*
