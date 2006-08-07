//
// secret.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using Gnome.Keyring;

namespace Gnome.Keyring {
	public class Test {
		static void Main ()
		{
			if (!Ring.Available) {
				Console.WriteLine ("The gnome-keyring-daemon cannot be reached.");
				return;
			}

			string deflt = Ring.GetDefaultKeyring ();
			Console.WriteLine ("The default keyring is '{0}'", deflt);
			Console.Write ("Other rings available: ");

			foreach (string s in Ring.GetKeyrings ()) {
				if (s != deflt)
					Console.Write ("'{0}' ", s);
			}
			Console.WriteLine ();

			// This is equivalent to...
			foreach (ItemData s in Ring.FindNetworkPassword ("gonzalo", null, null, null, null, null, 0)) {
				Console.WriteLine ("HERE");
				Console.WriteLine (s);
			}

			// ... this other search.
			Hashtable tbl = new Hashtable ();
			tbl ["user"] = "gonzalo";
			foreach (ItemData s in Ring.Find (ItemType.NetworkPassword, tbl)) {
				Console.WriteLine (s);
			}

			tbl = new Hashtable ();
			tbl ["user"] = "lalalito";
			tbl ["domain"] = "MiDomain";
			Console.WriteLine ("Creating item");
			int i = Ring.CreateItem (null, ItemType.NetworkPassword, "lala@pepe.com", tbl, "laclave", true);
			ItemData d2 = Ring.GetItemInfo (deflt, i);
			Ring.SetItemInfo (deflt, d2.ItemID, ItemType.NetworkPassword, "cambioesto@lalala", "otraclave");
			Hashtable atts = Ring.GetItemAttributes (deflt, i);
			foreach (string key in atts.Keys) {
				Console.WriteLine ("{0}: {1}", key, atts [key]);
			}

			atts ["object"] = "new attributes";
			Ring.SetItemAttributes (deflt, i, atts);
			Console.ReadLine ();

			Console.WriteLine ("Deleting it (ID = {0})", i);
			Ring.DeleteItem (Ring.GetDefaultKeyring (), i);
			Console.WriteLine ("Existing IDs...");
			foreach (int nn in Ring.ListItemIDs (deflt)) {
				Console.WriteLine (nn);
			}
		}
	}
}

