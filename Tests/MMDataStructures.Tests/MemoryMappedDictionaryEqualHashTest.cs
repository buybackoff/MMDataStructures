using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using MMDataStructures;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Summary description for MemoryMappedDictionaryEqualHashTest
    /// </summary>
    [TestFixture]
    public class MemoryMappedDictionaryEqualHashTest
    {
        private static MMDataStructures.Dictionary<Customer, int> _dict;
        private static MMDataStructures.Dictionary<Customer, Customer> _dict2;

        [SetUp()]
        public void InitializeDictionary()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            _dict = new MMDataStructures.Dictionary<Customer, int>(path);
            _dict2 = new MMDataStructures.Dictionary<Customer, Customer>(path);
        }

        [TearDown]
        public void CleanupDictionary()
        {
            _dict = null;
            _dict2 = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        class Customer
        {
            public string Name { get; set; }

            public override int GetHashCode()
            {
                return 1;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Customer)) return false;
                return string.Equals(Name, ((Customer)obj).Name);
            }
        }

        [Test]
        public void When_adding_two_key_objects_which_have_same_hash_verify_that_both_are_added()
        {
            Customer c1 = new Customer { Name = "a" };
            Customer c2 = new Customer { Name = "b" };

            _dict.Add(c1, 1);
            _dict.Add(c2, 2);

            Assert.AreEqual(2, _dict.Count);
            Assert.AreEqual(1, _dict[c1]);
            Assert.AreEqual(2, _dict[c2]);
        }

        [Test]
        public void When_removing_objects_verify_count()
        {
            Customer c1 = new Customer { Name = "a" };
            Customer c2 = new Customer { Name = "b" };

            _dict.Add(c1, 1);
            _dict.Add(c2, 2);

            _dict.Remove(c1);
            _dict.Remove(c2);
            Assert.AreEqual(0, _dict.Count);

            bool remove = _dict.Remove(c1);
            Assert.IsFalse(remove);
            remove = _dict.Remove(c2);
            Assert.IsFalse(remove);
        }

        [Test]
        public void When_adding_key_and_value_with_equal_hash_verify_that_both_are_added()
        {
            Customer c1 = new Customer { Name = "a" };
            Customer c2 = new Customer { Name = "b" };

            _dict2.Add(c1, c1);
            _dict2.Add(c2, c2);

            Assert.AreEqual(2, _dict2.Count);
            Assert.AreEqual(c1, _dict2[c1]);
            Assert.AreEqual(c2, _dict2[c2]);
        }

        [Test]
        public void When_removing_key_value_pairs_verify_dictionary_status()
        {
            Customer c1 = new Customer { Name = "a" };
            Customer c2 = new Customer { Name = "b" };
            _dict2.Add(c1, c1);
            _dict2.Add(c2, c2);

            KeyValuePair<Customer, Customer> kvp1 = new KeyValuePair<Customer, Customer>(c1, c1);
            KeyValuePair<Customer, Customer> kvp2 = new KeyValuePair<Customer, Customer>(c2, c2);

            Assert.IsTrue(_dict2.Remove(kvp1));
            Assert.IsFalse(_dict2.Remove(kvp1));

            Assert.IsTrue(_dict2.Remove(kvp2));
            Assert.IsFalse(_dict2.Remove(kvp2));
        }
    }
}
