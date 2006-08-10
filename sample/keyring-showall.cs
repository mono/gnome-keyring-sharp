//
// keyring-showall.cs
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

public class Test {
	static void Main ()
	{
		foreach (string s in Ring.GetKeyrings ()) {
			KeyringInfo kinfo = Ring.GetKeyringInfo (s);
			Console.WriteLine (kinfo);
			foreach (int id in Ring.ListItemIDs (s)) {
				ItemData item = Ring.GetItemInfo (s, id);
				Console.WriteLine ("  Item ID: {0}\n" +
						   "    Type: {1}\n" +
						   "    Secret: {2}\n" +
						   "    Attributes:",
						   item.ItemID, item.Type, item.Secret);
				Hashtable tbl = item.Attributes;
				foreach (string key in tbl.Keys) {
					Console.WriteLine ("      {0} =  {1}", key, tbl [key]);
				}
			}
			Console.WriteLine ();
		}
	}
}

