// 
// TestRing.cs
//  
// Author:
//       Christopher James Halse Rogers <<christopher.halse.rogers@canonical.com>>
// 
// Copyright (c) 2010 Christopher James Halse Rogers <christopher.halse.rogers@canonical.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.


using System;
using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;

namespace Gnome.Keyring
{
	public static class TestHelpers
	{
		public static void NotContains<T> (T toFind, ICollection<T> collection)
		{
			Assert.That (collection, new NotConstraint (new ContainsConstraint (toFind)));
		}
	}
	
	[TestFixture()]
	public class TestRing
	{
		[SetUp()]
		public void SetUp ()
		{
			try {
				Ring.CreateKeyring ("keyring1", "password");
			} catch (KeyringException e) {
				if (e.ResultCode != ResultCode.AlreadyExists) {
					throw;
				}
			}
			Ring.SetDefaultKeyring ("keyring1");
		}
		
		[TearDown()]
		public void TearDown ()
		{
			try {
				Ring.DeleteKeyring ("keyring1");
				Ring.SetDefaultKeyring ("login");
			} catch {
				// I don't care.
			}
			GC.Collect ();
		}
		
		[Test()]
		public void GetDefaultKeyringNameReturnsLogin ()
		{
			Assert.AreEqual ("keyring1", Ring.GetDefaultKeyring ());
		}
		
		[Test()]
		public void GetKeyringsListsAllKeyrings ()
		{
			Assert.Contains ("keyring1", Ring.GetKeyrings ());
		}
		
		[Test()]
		public void KeyringIsAvailable ()
		{
			Assert.IsTrue (Ring.Available);
		}
		
		[Test()]
		public void SetDefaultKeyringUpdatesGetDefaultKeyring ()
		{
			string prevDefault = Ring.GetDefaultKeyring ();
			Ring.CreateKeyring ("test1", "password");
			try {
				Ring.SetDefaultKeyring ("test1");
				Assert.AreEqual ("test1", Ring.GetDefaultKeyring ());
			} finally {
				Ring.DeleteKeyring ("test1");
				Ring.SetDefaultKeyring (prevDefault);
			}
		}
		
		[Test()]
		[ExpectedException (ExpectedMessage = "No such keyring", ExceptionType = typeof (KeyringException))]
		public void SetDefaultKeyringWithInvalidKeyringRaisesException ()
		{
			Ring.SetDefaultKeyring ("Keyring That Doesn't Exist");
		}
		
		[Test()]
		public void CreatedKeyringAppearsInKeyringList ()
		{
			Ring.ApplicationName = "Tests";
			string keyringName = "atestkeyring";
			Ring.CreateKeyring (keyringName, "password");
			Assert.Contains (keyringName, Ring.GetKeyrings ());
			Ring.DeleteKeyring (keyringName);
		}
		
		[Test()]
		[ExpectedException (ExpectedMessage = "Item already exists", ExceptionType = typeof (KeyringException))]
		public void CreatingTheSameKeyringTwiceRaisesException ()
		{
			string keyringName = "anothertestkeyring";
			Ring.CreateKeyring (keyringName, "password");
			try {
				Ring.CreateKeyring (keyringName, "password");
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void RemovingKeyringRemovesItFromKeyringList ()
		{
			string keyringName = "akeyring";
			Ring.CreateKeyring (keyringName, "password");
			Assert.Contains (keyringName, Ring.GetKeyrings ());
			Ring.DeleteKeyring (keyringName);
			TestHelpers.NotContains (keyringName, Ring.GetKeyrings ());
		}
		
		[Test()]
		public void GetKeyringInfoGetsCorrectName ()
		{
			string keyringName = "login";
			KeyringInfo info = Ring.GetKeyringInfo (keyringName);
			Assert.AreEqual (keyringName, info.Name);
		}
		
		[Test()]
		[Ignore ("Setting keyring properties is broken in libgnome-keyring.  Apparently no one uses this.")]
		public void SetKeyringInfoUpdatesLockTimeout ()
		{
			string keyringName = "testkeyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				KeyringInfo info = Ring.GetKeyringInfo (keyringName);
				info.LockTimeoutSeconds++;
				Assert.AreNotEqual (info.LockTimeoutSeconds, Ring.GetKeyringInfo (keyringName).LockTimeoutSeconds);
				
				Ring.Unlock (keyringName, null);
				Ring.SetKeyringInfo (keyringName, info);
				Assert.AreEqual (info.LockTimeoutSeconds, Ring.GetKeyringInfo (keyringName).LockTimeoutSeconds);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		[Ignore ("Setting keyring properties is broken in libgnome-keyring.  Apparently no one uses this.")]
		public void SetKeyringLockOnIdleUpdatesInfo ()
		{
			string keyringName = "theamazingtestkeyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				KeyringInfo info = Ring.GetKeyringInfo (keyringName);
				info.LockOnIdle = !info.LockOnIdle;
				Assert.AreNotEqual (info.LockOnIdle, Ring.GetKeyringInfo (keyringName).LockOnIdle);
				
				Ring.SetKeyringInfo (keyringName, info);
				Assert.AreEqual (info.LockOnIdle, Ring.GetKeyringInfo (keyringName).LockOnIdle);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void TestIdListForLoginKeyringIsNonEmpty ()
		{
			Assert.IsNotEmpty (Ring.ListItemIDs ("login"));
		}
		
		[Test()]
		public void TestIdListContainsSaneIds ()
		{
			foreach (int i in Ring.ListItemIDs ("login")) {
				Assert.GreaterOrEqual (i, 0);
			}
			CollectionAssert.AllItemsAreUnique (Ring.ListItemIDs ("login"));
		}
		
		[Test()]
		public void CreatedItemIdExistsInIdList ()
		{
			string keyringName = "testifu";
			Ring.CreateKeyring (keyringName, "password");
			
			Hashtable attributes = new Hashtable ();
			attributes["name"] = "woot";
			attributes["banana"] = "some other value";
			attributes["eggplant"] = "aubergine";
			attributes["apple"] = 25;
			
			try {
				int id = Ring.CreateItem (keyringName, ItemType.Note, "Random note", attributes, "reallysecret", false);
				CollectionAssert.Contains (Ring.ListItemIDs (keyringName), id);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		/*
		 * It seems that there's no way to stop gnome-keyring-daemon from prompting the user to unlock.
		 * So we won't get an AccessDenied exception here; we'll block until the user has dealt with the unlock dialog.
		 */
		[Test()]
		[Ignore ("Requires user interaction")]
		public void AccessingALockedKeyringPromptsToUnlock ()
		{
			string keyringName = "akeyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				Ring.Lock (keyringName);
				Ring.CreateItem (keyringName, ItemType.Note, "Random note", new Hashtable (), "reallysecret", false);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void DeletingItemRemovesItFromIdList ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				int id = Ring.CreateItem (keyringName, ItemType.Note, "test data", new Hashtable (), "secret", false);
				CollectionAssert.Contains (Ring.ListItemIDs (keyringName), id);
				Ring.DeleteItem (keyringName, id);
				CollectionAssert.DoesNotContain (Ring.ListItemIDs (keyringName), id);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void DataOfAddedItemPersists ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				int id = Ring.CreateItem (keyringName, ItemType.Note, "test data", new Hashtable (), "secret", false);
				
				ItemData item = Ring.GetItemInfo (keyringName, id);
				Assert.AreEqual ("test data", (string)item.Attributes["name"]);
				Assert.AreEqual ("secret", item.Secret);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void SetItemDataPersists ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				int id = Ring.CreateItem (keyringName, ItemType.Note, "test", new Hashtable (), "secret", false);
				
				Ring.SetItemInfo (keyringName, id, ItemType.GenericSecret, "newdisplayname", "newsecret");
				ItemData item = Ring.GetItemInfo (keyringName, id);
				Assert.AreEqual ("newdisplayname", (string)item.Attributes["name"]);
				Assert.AreEqual ("newsecret", item.Secret);
				Assert.AreEqual (ItemType.GenericSecret, item.Type);
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void GetItemAttributesReturnsCreatedValues ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				Hashtable attributes = new Hashtable ();
				attributes["stringAttr"] = "astring";
				attributes["uintAttr"] = 42;
				int id = Ring.CreateItem (keyringName, ItemType.Note, "test", attributes, "secret", false);
				
				CollectionAssert.IsSubsetOf (attributes, Ring.GetItemAttributes (keyringName, id));
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void SetItemAttributesPersists ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				int id = Ring.CreateItem (keyringName, ItemType.Note, "test", new Hashtable (), "secret", false);

				Hashtable attributes = new Hashtable ();
				attributes["stringAttr"] = "astring";
				attributes["meaning"] = 42;
				attributes["UTF8"] = "♪ “The sun is a mass of incandescent gas” ♫";
				
				Ring.SetItemAttributes (keyringName, id, attributes);
				
				CollectionAssert.IsSubsetOf (attributes, Ring.GetItemAttributes (keyringName, id));
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void CreatedNetworkPasswordSetsAppropriateData ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			
			try {
				Hashtable data = new Hashtable ();
				data["user"] = "raof";
				data["domain"] = "divination";
				data["server"] = "jeeves";
				data["object"] = "subject";
				data["protocol"] = "droid";
				data["authtype"] = "smtp";
				data["port"] = 42;
				
				//Password is stored in the secret.
				int id = Ring.CreateOrModifyNetworkPassword (keyringName, (string)data["user"], (string)data["domain"], (string)data["server"],
					(string)data["object"], (string)data["protocol"], (string)data["authtype"], (int)data["port"], "password");
				
				CollectionAssert.IsSubsetOf (data, Ring.GetItemAttributes (keyringName, id));
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void FindNetworkPasswordByDomainFindsAppropriateIDs ()
		{
			string keyringName = "keyring";
			Ring.CreateKeyring (keyringName, "password");
			List<int> ids = new List<int> ();
			
			try {
				ids.Add (Ring.CreateOrModifyNetworkPassword (keyringName, "user", "domain", "server", "object", "protocol", "authtype", 42, "password"));
				Ring.CreateOrModifyNetworkPassword (keyringName, "user2", "d3omain", "4server", "o5bject", "proto6col", "3authtype", 49, "password");
				Ring.CreateItem (keyringName, ItemType.Note, "I'm not a network password", new Hashtable (), "secret", false);
				ids.Add (Ring.CreateOrModifyNetworkPassword (keyringName, "u3ser", "domain", "server", "object", "protocol", "authtype", 42, "password"));
				ids.Add (Ring.CreateOrModifyNetworkPassword (keyringName, "use4r", "domain", "server", "object", "protocol", "authtype", 42, "password"));

				CollectionAssert.AreEquivalent (ids, Ring.FindNetworkPassword (null, "domain", null, null, null, null, 0).
					Where ((NetItemData data) => data.Keyring == keyringName).
					Select ((NetItemData data) => data.ItemID).ToList ());
			} finally {
				Ring.DeleteKeyring (keyringName);
			}
		}
		
		[Test()]
		public void FindItemByAttributeOnlyFindsMatchingIDs ()
		{
			string keyringName = "keyring1";
			
			List<int> correct_ids = new List<int> ();
			List<int> incorrect_ids = new List<int> ();
			
			Hashtable correctAttr = new Hashtable ();
			correctAttr["banana"] = "a fruit";
			Hashtable incorrectAttr = new Hashtable ();
			incorrectAttr["banana"] = "a fish";
			
			correct_ids.Add (Ring.CreateItem (keyringName, ItemType.Note, "a note", correctAttr, "secret", false));
			incorrect_ids.Add (Ring.CreateItem (keyringName, ItemType.GenericSecret, "not a note", incorrectAttr, "secret", false));
			correct_ids.Add (Ring.CreateItem (keyringName, ItemType.Note, "another note", correctAttr, "notsecret", false));
			correct_ids.Add (Ring.CreateItem (keyringName, ItemType.Note, "a third note", correctAttr, "reallysecret", false));
			incorrect_ids.Add (Ring.CreateOrModifyNetworkPassword (keyringName, "use4r", "domain", "server", "object", "protocol", "authtype", 42, "password"));

			CollectionAssert.IsSubsetOf (Ring.Find (ItemType.Note, correctAttr).
				Where ((data) => data.Keyring == keyringName).
				Select ((data) => data.ItemID).ToList (), correct_ids);
			foreach (var id in incorrect_ids) {
				CollectionAssert.DoesNotContain (Ring.Find (ItemType.Note, correctAttr).
					Where ((data) => data.Keyring == keyringName).
					Select ((data) => data.ItemID), id);
			}
		}
	}
}
