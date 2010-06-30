AC_PREREQ(2.57)
AC_INIT(src/Gnome.Keyring/Ring.cs)
AC_CANONICAL_SYSTEM

AC_PROG_CC
AC_PROG_LIBTOOL

API_VERSION=1.0
VERSION=$API_VERSION.2

AC_SUBST(API_VERSION)
AC_SUBST(VERSION)

AM_INIT_AUTOMAKE(gnome-keyring-sharp, $VERSION)
AM_MAINTAINER_MODE

AC_PROG_INSTALL

AC_PATH_PROG(CSC, csc, no)
AC_PATH_PROG(GMCS, gmcs, no)
AC_PATH_PROG(MCS, mcs, no)
AC_PATH_PROG(RUNTIME, mono, no)
CS="C#"

if test "x$CSC" = "xno" ; then
	if test "x$GMCS" = "xno" ; then
		if test "x$MCS" = "xno" ; then
			AC_MSG_ERROR([No $CS compiler found])
		else
			CSC=$MCS
			ENABLE_DBUS="no"
		fi
	else
		CSC=$GMCS
	fi
fi

#
# The GAC tool
#

AC_PATH_PROG(GACUTIL, gacutil, no)
if test "x$GACUTIL" = "xno" ; then
	AC_MSG_ERROR([No gacutil tool found])
fi

AC_SUBST(GACUTIL)

GACUTIL_FLAGS='/gacdir $(DESTDIR)$(prefix)'
AC_SUBST(GACUTIL_FLAGS)


PKG_CHECK_MODULES(GLIB_SHARP, [glib-sharp-2.0])
PKG_CHECK_MODULES(GNOME_KEYRING, [gnome-keyring-1])

AC_SUBST(CSFLAGS)
AC_SUBST(GLIB_SHARP_LIBS)
AC_SUBST(GNOME_KEYRING_CFLAGS)
AC_SUBST(GNOME_KEYRING_LIBS)

AC_ARG_ENABLE(monodoc, 
	      [AC_HELP_STRING([--enable-monodoc],[install monodoc documents default: yes])],
	      [ENABLE_MONODOC="$enableval"])

if test "x$ENABLE_MONODOC" != "xno" ; then
	AC_PATH_PROG(MDASSEMBLER, mdassembler, no)
	AC_PATH_PROG(MONODOCER, monodocer, no)

	if test "x$MONODOCER" = "xno" -o "x$MDASSEMBLER" = "xno"; then
		if test "x$ENABLE_MONODOC" = "xyes" ; then
			AC_MSG_ERROR([monodoc can't be installed even though it was explicitly enabled. mdassembler or monodocer not found.])
		else
			enable_monodoc=no
			doc_sources_dir=
		fi
	else
		enable_monodoc=yes
		doc_sources_dir="`pkg-config --variable=sourcesdir monodoc`"
	fi
fi

AM_CONDITIONAL(ENABLE_MONODOC, test "x$enable_monodoc" = "xyes")
AC_SUBST(CSC)
AC_SUBST(RUNTIME)
AC_SUBST(MDASSEMBLER)
AC_SUBST(MONODOCER)
AC_OUTPUT([
	Makefile
	src/Makefile
	src/gnome-keyring-sharp-1.0.pc
	src/Gnome.Keyring/Makefile
	src/Gnome.Keyring/AssemblyInfo.cs
	gnome-keyring-sharp-glue/Makefile
	docs/Makefile
	sample/Makefile
])

echo "   * $CS compiler: $CSC"
echo "   * Documentation build enabled: $enable_monodoc "
if test "x$enable_monodoc" = "xyes" -a "x$doc_sources_dir" != "x$prefix/lib/monodoc/sources"; then
  echo "      WARNING: The install prefix is different than the monodoc prefix."
  echo "               Monodoc will not be able to load the documentation."
fi

