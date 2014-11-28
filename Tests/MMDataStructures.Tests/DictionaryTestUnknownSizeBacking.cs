using System;
using System.Collections.Generic;
using MMDataStructures.DictionaryBacking;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Summary description for DictionaryTestUnknownSizeBacking
    /// </summary>
    [TestFixture]
    public class DictionaryTestUnknownSizeBacking
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        

        private class Customer
        {
            public string Name { get; set; }

            public override int GetHashCode()
            {
                return 1;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Customer)) return false;
                return string.Equals(Name, ((Customer) obj).Name);
            }
        }

        [Test]
        public void AddValues_VerifyValues()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<string, string> backingFile = new BackingUnknownSize<string, string>(path, 2000000);
            Dictionary<string, string> dict = new Dictionary<string, string>(backingFile);

            string prevKey = null;
            string prevVal = null;
            for (int i = 0; i < 500000; i++)
            {
                string key = Guid.NewGuid().ToString();
                string value = Guid.NewGuid().ToString();
                dict.Add(key, value);

                if (prevKey != null)
                {
                    string result = dict[prevKey];
                    Assert.AreEqual(prevVal, result);
                }
                prevKey = key;
                prevVal = value;
            }
        }

        [Test]
        public void AddTwoValues_CheckConsistency()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<Customer, string> backingFile = new BackingUnknownSize<Customer, string>(path, 100);
            Dictionary<Customer, string> dict = new Dictionary<Customer, string>(backingFile);

            Customer c1 = new Customer {Name = "Mikael"};
            Customer c2 = new Customer {Name = "Svenson"};

            dict.Add(c1, "test");
            dict.Add(c2, "test2");
            string result = dict[c1];
            Assert.AreEqual("test", result);
            result = dict[c2];
            Assert.AreEqual("test2", result);
        }

        [Test]
        public void AddThreeItems_RemoveMiddleItem_CheckConsistency()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<Customer, string> backingFile = new BackingUnknownSize<Customer, string>(path, 100);
            Dictionary<Customer, string> dict = new Dictionary<Customer, string>(backingFile);

            Customer c1 = new Customer {Name = "Mikael"};
            Customer c2 = new Customer {Name = "Svenson"};
            Customer c3 = new Customer {Name = "Boss"};

            dict.Add(c1, "test");
            dict.Add(c2, "test2");
            dict.Add(c3, "test3");

            var result = dict.Remove(c2);
            Assert.IsTrue(result);
            result = dict.Remove(c2);
            Assert.IsFalse(result);
            dict.Add(c2, "test2");
            result = dict.Remove(c2);
            Assert.IsTrue(result);

            var res2 = dict[c3];
            Assert.AreEqual("test3", res2);
        }

        [Test]
        public void AddThreeItems_RemoveFirstItem_CheckConsistency()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<Customer, string> backingFile = new BackingUnknownSize<Customer, string>(path, 100);
            Dictionary<Customer, string> dict = new Dictionary<Customer, string>(backingFile);

            Customer c1 = new Customer {Name = "Mikael"};
            Customer c2 = new Customer {Name = "Svenson"};
            Customer c3 = new Customer {Name = "Boss"};

            dict.Add(c1, "test");

            var result = dict.Remove(c1);
            Assert.IsTrue(result);

            dict.Add(c2, "test2");
            dict.Add(c3, "test3");
            dict.Add(c1, "test");

            result = dict.Remove(c1);
            Assert.IsTrue(result);
            result = dict.Remove(c1);
            Assert.IsFalse(result);
            dict.Add(c1, "test");
            result = dict.Remove(c1);
            Assert.IsTrue(result);

            var res2 = dict[c3];
            Assert.AreEqual("test3", res2);
        }

        [Test]
        public void IterateAllItems_CheckConsistency()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<Customer, string> backingFile = new BackingUnknownSize<Customer, string>(path, 100);
            Dictionary<Customer, string> dict = new Dictionary<Customer, string>(backingFile);

            Customer c1 = new Customer {Name = "Mikael"};
            Customer c2 = new Customer {Name = "Svenson"};
            Customer c3 = new Customer {Name = "Boss"};

            dict.Add(c1, "Mikael");
            dict.Add(c2, "Svenson");
            dict.Add(c3, "Boss");

            int count = 0;
            foreach (KeyValuePair<Customer, string> pair in dict)
            {
                Assert.AreEqual(pair.Key.Name, pair.Value);
                count++;
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void IterateAllItems_CheckConsistency2()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            BackingUnknownSize<int, int> backingFile = new BackingUnknownSize<int, int>(path, 100);
            Dictionary<int, int> dict = new Dictionary<int, int>(backingFile);

            dict.Add(1, 1);
            dict.Add(2, 2);
            dict.Add(3, 3);

            int count = 0;
            foreach (KeyValuePair<int, int> pair in dict)
            {
                Assert.AreEqual(pair.Key, pair.Value);
                count++;
            }
            Assert.AreEqual(3, count);
        }
    }
}