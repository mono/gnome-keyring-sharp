SUBDIRS=Gnome.Keyring

ASSEMBLY_NAME=Gnome.Keyring
ASSEMBLY=$(ASSEMBLY_NAME).dll
SNK=$(ASSEMBLY_NAME).snk

pkgconfigdir=$(libdir)/pkgconfig
CSFLAGS+= -debug+ -debug:full -nologo -r:Mono.Posix.dll

pkgconfig_DATA = gnome-keyring-sharp-1.0.pc

CLEANFILES = $(ASSEMBLY_NAME).*

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

all: $(ASSEMBLY)

$(ASSEMBLY): $(gnomekeyring_build_sources) $(SNK)
	$(CSC) $(CSFLAGS) $(gnomekeyring_references) -target:library -out:$@ $(gnomekeyring_build_sources)

$(SNK): $(top_srcdir)/$(SNK)
	cp $(top_srcdir)/$(SNK) .

### GAC install rules ###

install-data-local:
	echo "$(GACUTIL) /i $(ASSEMBLY) $(GACUTIL_FLAGS)"; \
	$(GACUTIL) /i $(ASSEMBLY) /package $(PACKAGE)-$(API_VERSION) $(GACUTIL_FLAGS) || exit 1;

uninstall-local:
	echo "$(GACUTIL) /u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)"; \
	$(GACUTIL) /u $(ASSEMBLY_NAME) /package $(PACKAGE)-$(API_VERSION) $(GACUTIL_FLAGS) || exit 1;

