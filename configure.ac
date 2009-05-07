dnl configure.ac for Mono.Google library
AC_PREREQ(2.57)
AC_INIT(src/Gnome.Keyring/Ring.cs)
AC_CANONICAL_SYSTEM

API_VERSION=1.0
VERSION=$API_VERSION.1

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

#
# Use D-Bus as a fallback to get the keyring socket address
#
AC_ARG_ENABLE(dbus, 
	      [AC_HELP_STRING([--enable-dbus],[compile with D-Bus support default: yes])],
	      ENABLE_DBUS="$enableval")
AM_CONDITIONAL(ENABLE_DBUS, test "x$ENABLE_DBUS" != "xno")

CSFLAGS=
DBUS_LIBS=
if test "x$ENABLE_DBUS" != "xno" ; then
	PKG_CHECK_MODULES(DBUS, ndesk-dbus-1.0 >= 0.4, HAVE_DBUS="yes", HAVE_DBUS="no")

	if test "x$HAVE_DBUS" = "xno"; then
	  AC_MSG_ERROR($DBUS_PKG_ERRORS: consider passing --enable-dbus=no to configure)
	fi

	CSFLAGS=" -d:WITH_DBUS "
fi
AC_SUBST(CSFLAGS)
AC_SUBST(DBUS_LIBS)

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
	docs/Makefile
	sample/Makefile
])

echo "   * $CS compiler: $CSC"
echo "   * Documentation build enabled: $enable_monodoc "
if test "x$enable_monodoc" = "xyes" -a "x$doc_sources_dir" != "x$prefix/lib/monodoc/sources"; then
  echo "      WARNING: The install prefix is different than the monodoc prefix."
  echo "               Monodoc will not be able to load the documentation."
fi

