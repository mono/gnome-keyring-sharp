SUBDIRS=Gnome.Keyring

ASSEMBLY_NAME=Gnome.Keyring
ASSEMBLY=$(ASSEMBLY_NAME).dll
SNK=$(ASSEMBLY_NAME).key

pkgconfigdir=$(libdir)/pkgconfig
CSFLAGS+= -debug+ -debug:full -nologo -r:Mono.Posix.dll

gnomekeyringdir = $(prefix)/lib/Gnome.Keyring
pkgconfig_DATA = gnome-keyring-sharp.pc
gnomekeyring_SCRIPTS = Gnome.Keyring.dll

CLEANFILES = $(ASSEMBLY_NAME).*

# 
gnomekeyring_references = $(DBUS_LIBS)

gnomekeyring_sources  = \
			Gnome.Keyring/AccessRights.cs \
			Gnome.Keyring/AttributeType.cs \
			Gnome.Keyring/GenericItemData.cs \
			Gnome.Keyring/ItemACL.cs \
			Gnome.Keyring/ItemData.cs \
			Gnome.Keyring/ItemType.cs \
			Gnome.Keyring/KeyringException.cs \
			Gnome.Keyring/KeyringInfo.cs \
			Gnome.Keyring/NetItemData.cs \
			Gnome.Keyring/NoteItemData.cs \
			Gnome.Keyring/Operation.cs \
			Gnome.Keyring/RequestMessage.cs \
			Gnome.Keyring/ResponseMessage.cs \
			Gnome.Keyring/ResultCode.cs \
			Gnome.Keyring/Ring.cs

gnomekeyring_build_sources = $(addprefix $(srcdir)/, $(gnomekeyring_sources)) $(addprefix $(top_builddir)/src/Gnome.Keyring/, AssemblyInfo.cs)

$(ASSEMBLY): $(gnomekeyring_build_sources) $(SNK)
	$(CSC) $(CSFLAGS) $(gnomekeyring_references) -target:library -out:$@ $(gnomekeyring_build_sources)

$(SNK): $(top_srcdir)/$(SNK)
	cp $(top_srcdir)/$(SNK) .

### GAC install rules ###

install-data-local:
	echo "$(GACUTIL) /i $(ASSEMBLY) $(GACUTIL_FLAGS)"; \
	$(GACUTIL) /i $(ASSEMBLY) $(GACUTIL_FLAGS) || exit 1;

uninstall-local:
	echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)"; \
	$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS) || exit 1;

