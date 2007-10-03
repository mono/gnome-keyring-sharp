if ENABLE_MONODOC
SOURCESDIR=$(libdir)/monodoc/sources
TARGETS=Gnome.Keyring.tree Gnome.Keyring.zip Gnome.Keyring.source
UPDATER = $(MONODOCER) -path:Gnome.Keyring $(UPDATE_ASSEMBLIES)
else
SOURCESDIR=$(datadir)
TARGETS=
UPDATER = echo "Documentation build is disabled."
endif

NAMESPACES=Gnome.Keyring
UPDATE_ASSEMBLIES = -assembly:$(top_builddir)/src/Gnome.Keyring.dll

docsdir=$(SOURCESDIR)
docs_DATA=$(TARGETS)

CLEANFILES=Gnome.Keyring.tree Gnome.Keyring.zip
EXTRA_DIST=Gnome.Keyring.source

Gnome.Keyring: Gnome.Keyring.tree

Gnome.Keyring.tree Gnome.Keyring.zip: $(srcdir)/Gnome.Keyring/*.xml $(srcdir)/Gnome.Keyring/*/*.xml
	mdassembler --ecma $(srcdir)/Gnome.Keyring -o Gnome.Keyring

update: 
	$(UPDATER)

update-delete: 
	$(UPDATER) --delete

merge:
	monodoc --merge-changes ~/.config/monodoc/changeset.xml .

dist-hook:
	mkdir -p $(distdir)/Gnome.Keyring
	cp $(srcdir)/Gnome.Keyring/*.xml $(distdir)/Gnome.Keyring/
	for i in $(NAMESPACES); do				\
		mkdir -p $(distdir)/Gnome.Keyring/$$i;			\
		cp $(srcdir)/Gnome.Keyring/$$i/*.xml $(distdir)/Gnome.Keyring/$$i;	\
	done

