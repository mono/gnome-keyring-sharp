SUBDIRS=Gnome.Keyring

pkgconfigdir=$(libdir)/pkgconfig
MCSFLAGS= -debug+ -debug:full -nologo -r:Mono.Posix.dll

gnomekeyringdir = $(prefix)/lib/Gnome.Keyring
pkgconfig_DATA = gnome-keyring-sharp.pc
gnomekeyring_SCRIPTS = Gnome.Keyring.dll

CLEANFILES = Gnome.Keyring.dll*

# 
gnomekeyring_references=

gnomekeyring_sources  = \
			Gnome.Keyring/AttributeType.cs \
			Gnome.Keyring/ItemData.cs \
			Gnome.Keyring/ItemType.cs \
			Gnome.Keyring/KeyringException.cs \
			Gnome.Keyring/NetItemData.cs \
			Gnome.Keyring/Operation.cs \
			Gnome.Keyring/RequestMessage.cs \
			Gnome.Keyring/ResponseMessage.cs \
			Gnome.Keyring/ResultCode.cs \
			Gnome.Keyring/Ring.cs

gnomekeyring_build_sources = $(addprefix $(srcdir)/, $(gnomekeyring_sources)) $(addprefix $(top_builddir)/src/Gnome.Keyring/, AssemblyInfo.cs)

Gnome.Keyring.dll: $(gnomekeyring_build_sources)
	$(MCS) $(MCSFLAGS) $(gnomekeyring_references) /target:library /out:$@ $(gnomekeyring_build_sources)

