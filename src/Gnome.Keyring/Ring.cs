//
// Gnome.Keyring.Ring.cs
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
using System.IO;
using System.Net;
using System.Net.Sockets;

using Mono.Unix;
using Mono.Unix.Native;

namespace Gnome.Keyring {
	public class Ring {
		static string appname;

		public static string ApplicationName {
			get { return appname; }
			set {
				if (value == null || value == "")
					throw new ArgumentException ("Cannot be null or empty", "value");
				appname = value;
			}
		}

		public static bool Available {
			get {
				Socket sock = Connect ();
				if (sock != null) {
					sock.Close ();
					return true;
				}
				return false;
			}
		}

		static Socket Connect ()
		{
			string filename = Environment.GetEnvironmentVariable ("GNOME_KEYRING_SOCKET");
			if (filename == null || filename == "")
				return null;

			EndPoint ep = new UnixEndPoint (filename);
			Socket sock = new Socket (AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
			try {
				sock.Connect (ep);
			} catch (Exception) {
				sock.Close ();
				return null;
			}
			return sock;
		}

		static int GetInt32 (Socket sock)
		{
			byte [] cuatro = new byte [4];
			if (sock.Receive (cuatro) != 4)
				throw new KeyringException (ResultCode.IOError);
			return (cuatro [3] + (cuatro [2] << 8) + (cuatro [1] << 16) + (cuatro [0] << 24));
		}

		static byte [] one = new byte [1];
		static ResponseMessage SendRequest (MemoryStream stream)
		{
			Socket sock = Connect ();
			if (sock == null)
				throw new KeyringException (ResultCode.NoKeyringDaemon);

			try {
				sock.Send (one); // Credentials byte
				byte [] buffer = stream.ToArray ();
				sock.Send (buffer);
				int packet_size = GetInt32 (sock) - 4;
				if (packet_size < 0)
					throw new KeyringException (ResultCode.IOError);
				byte [] response = new byte [packet_size];
				int nbytes = sock.Receive (response);
				if (nbytes != response.Length)
					throw new KeyringException (ResultCode.IOError);
				ResponseMessage resp = new ResponseMessage (response);
				ResultCode result = (ResultCode) resp.GetInt32 ();
				if (result != 0)
					throw new KeyringException (result);

				return resp;
			} finally {
				sock.Close ();
			}
		}

		public static void LockAll ()
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.LockAll);
			SendRequest (req.Stream);
		}

		public static void SetDefaultKeyring (string newKeyring)
		{
			if (newKeyring == null)
				throw new ArgumentNullException ("newKeyring");
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.SetDefaultKeyring, newKeyring);
			SendRequest (req.Stream);
		}

		public static string GetDefaultKeyring ()
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.GetDefaultKeyring);
			ResponseMessage resp = SendRequest (req.Stream);
			return resp.GetString ();
		}

		public static string [] GetKeyrings ()
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.ListKeyrings);
			ResponseMessage resp = SendRequest (req.Stream);
			return resp.GetStringList ();
		}

		public static void CreateKeyring (string name, string password)
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.CreateKeyring, name, password);
			SendRequest (req.Stream);
		}

		public static void Lock (string keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException ("keyring");

			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.LockKeyring, keyring);
			SendRequest (req.Stream);
		}

		public static void Unlock (string keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException ("keyring");

			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.UnlockKeyring, keyring);
			SendRequest (req.Stream);
		}

		public static void DeleteKeyring (string keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException ("keyring");

			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.DeleteKeyring, keyring);
			SendRequest (req.Stream);
		}

		public static int [] ListItemIDs (string keyring)
		{
			if (keyring == null)
				throw new ArgumentNullException ("keyring");

			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.ListItems, keyring);
			ResponseMessage resp = SendRequest (req.Stream);
			int len = resp.GetInt32 ();
			int [] result = new int [len];
			for (int i = 0; i < len; i++) {
				result [i] = resp.GetInt32 ();
			}

			return result;
		}

		public static ItemData [] Find (ItemType type, Hashtable atts)
		{
			if (atts == null)
				throw new ArgumentNullException ("atts");
			RequestMessage req = new RequestMessage ();
			req.StartOperation (Operation.Find);
			req.Write ((int) type);
			req.WriteAttributes (atts);
			req.EndOperation ();

			ResponseMessage resp = SendRequest (req.Stream);
			ArrayList list = new ArrayList ();
			while (resp.DataAvailable) {
				ItemData found = ItemData.GetInstanceFromItemType (type);
				found.Keyring = resp.GetString ();
				found.ItemID = resp.GetInt32 ();
				found.Secret = resp.GetString ();
				found.Attributes = new Hashtable ();
				resp.ReadAttributes (found.Attributes);
				found.SetValuesFromAttributes ();
				list.Add (found);
			}

			return (ItemData []) list.ToArray (typeof (ItemData));
		}

		public static NetItemData [] FindNetworkPassword (string user, string domain, string server, string obj,
									string protocol, string authtype, int port)
		{
			RequestMessage req = new RequestMessage ();
			req.StartOperation (Operation.Find);
			req.Write ((int) ItemType.NetworkPassword);
			Hashtable tbl = new Hashtable ();
			tbl ["user"] = user;
			tbl ["domain"] = domain;
			tbl ["server"] = server;
			tbl ["object"] = obj;
			tbl ["protocol"] = protocol;
			tbl ["authtype"] = authtype;
			if (port != 0)
				tbl ["port"] = port;
			req.WriteAttributes (tbl);
			req.EndOperation ();

			ResponseMessage resp = SendRequest (req.Stream);
			ArrayList list = new ArrayList ();
			while (resp.DataAvailable) {
				NetItemData found = new NetItemData ();
				found.Keyring = resp.GetString ();
				found.ItemID = resp.GetInt32 ();
				found.Secret = resp.GetString ();
				found.Attributes = new Hashtable ();
				resp.ReadAttributes (found.Attributes);
				found.SetValuesFromAttributes ();
				list.Add (found);
			}

			return (NetItemData []) list.ToArray (typeof (NetItemData));
		}

		public static int CreateItem (string keyring, ItemType type, string displayName, Hashtable attributes,
						string secret, bool updateIfExists)
		{
			RequestMessage req = new RequestMessage ();
			req.StartOperation (Operation.CreateItem);
			req.Write (keyring);
			req.Write (displayName);
			req.Write (secret);
			req.WriteAttributes (attributes);
			req.Write ((int) type);
			req.Write (updateIfExists ? 1 : 0);
			req.EndOperation ();
			ResponseMessage resp = SendRequest (req.Stream);
			return resp.GetInt32 ();
		}

		public static void DeleteItem (string keyring, int id)
		{
			if (keyring == null)
				throw new ArgumentNullException ("keyring");

			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.DeleteItem, keyring, id);
			SendRequest (req.Stream);
		}

		public static int CreateOrModifyNetworkPassword (string keyring, string user, string domain, string server, string obj,
								string protocol, string authtype, int port, string password)
		{
			Hashtable tbl = new Hashtable ();
			tbl ["user"] = user;
			tbl ["domain"] = domain;
			tbl ["server"] = server;
			tbl ["object"] = obj;
			tbl ["protocol"] = protocol;
			tbl ["authtype"] = authtype;
			if (port != 0)
				tbl ["port"] = port;

			string display_name;
			if (port != 0)
				display_name = String.Format ("{0}@{1}:{3}/{2}", user, server, obj, port);
			else
				display_name = String.Format ("{0}@{1}/{2}", user, server, obj);

			return CreateItem (keyring, ItemType.NetworkPassword, display_name, tbl, password, true);
		}

		public static ItemData GetItemInfo (string keyring, int id)
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.GetItemInfo, keyring, id);
			ResponseMessage resp = SendRequest (req.Stream);
			ItemType itype = (ItemType) resp.GetInt32 ();
			ItemData item = ItemData.GetInstanceFromItemType (itype);
			string name = resp.GetString ();
			string secret = resp.GetString ();
			long mtime = (resp.GetInt32 () << 32) + resp.GetInt32 ();
			long ctime = (resp.GetInt32 () << 32) + resp.GetInt32 ();
			item.Keyring = keyring;
			item.ItemID = id;
			item.Secret = secret;
			Hashtable tbl = new Hashtable ();
			tbl ["name"] = name;
			tbl ["keyring_ctime"] = NativeConvert.FromTimeT (ctime);
			tbl ["keyring_mtime"] = NativeConvert.FromTimeT (mtime);
			item.Attributes = tbl;
			item.SetValuesFromAttributes ();
			return item;
		}

		public static Hashtable GetItemAttributes (string keyring, int id)
		{
			RequestMessage req = new RequestMessage ();
			req.CreateSimpleOperation (Operation.GetItemAttributes, keyring, id);
			ResponseMessage resp = SendRequest (req.Stream);
			Hashtable tbl = new Hashtable ();
			int count = resp.GetInt32 ();
			for (int i = 0; i < count; i++) {
				string key = resp.GetString ();
				AttributeType atype = (AttributeType) resp.GetInt32 ();
				if (atype == AttributeType.String) {
					tbl [key] = (string) resp.GetString ();
				} else if (atype == AttributeType.UInt32) {
					tbl [key] = (int) resp.GetInt32 ();
				} else {
					throw new Exception ("This should not happen: "  + atype);
				}
			}
			return tbl;
		}

		/*
		* TODO:
			GetKeyringInfo,
			SetKeyringInfo,
			SetItemInfo,
			SetItemAttributes,
			GetItemACL,
			SetItemACL
		*/
	}
}
