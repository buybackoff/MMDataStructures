using System;
using System.Collections.Generic;
using MMDataStructures;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    public class TestFixture
    {
        public string Name { get; set; }

        public TestFixture()
        {
        }

        public TestFixture(int length)
        {
            Name = "".PadLeft(length, 'a');
        }

        public override int GetHashCode()
        {
            if (!string.IsNullOrEmpty(Name)) return Name.GetHashCode();
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TestFixture)) return false;
            return string.Equals(((TestFixture)obj).Name, Name);
        }
    }

    /// <summary>
    /// Summary description for MemoryMappedDictionaryTest
    /// </summary>
    [TestFixture]
    public class MemoryMappedDictionaryTestClassValue
    {
        

        private static MMDataStructures.Dictionary<int, TestFixture> _dict;

       
        public static void InitializeDictionary()
        {
            if (_dict == null)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                _dict = new MMDataStructures.Dictionary<int, TestFixture>(path);
            }
        }

        [SetUp()]
        public void ClearDictionary()
        {
            InitializeDictionary();
            _dict.Clear();
        }

        [Test]
        public void When_adding_an_item_verify_that_the_item_can_be_retreived()
        {
            TestFixture t = new TestFixture(10);
            _dict[0] = t;
            Assert.AreEqual(t, _dict[0]);
        }

        [Test]
        public void When_adding_an_item_verify_that_the_item_count()
        {
            TestFixture t = new TestFixture(10);
            _dict[0] = t;
            Assert.AreEqual(1, _dict.Count);
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void When_retrieving_a_non_existing_key_throw_KeyNotFoundException()
        {
            TestFixture a = _dict[0];
        }

        [Test]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void When_deleting_a_key_thrown_KeyNotFoundException_when_accessing_it_afterwards()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(345, t);
            bool removed = _dict.Remove(345);
            Assert.IsTrue(removed);
            Assert.AreEqual(0, _dict.Count);
            t = _dict[345];
        }

        [Test]
        public void When_checking_if_an_existing_key_exists_return_true()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(11, t);
            Assert.IsTrue(_dict.ContainsKey(11));
        }

        [Test]
        public void When_checking_if_a_nonexisting_key_exists_return_false()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(1, t);
            Assert.IsFalse(_dict.ContainsKey(10));
        }

        [Test]
        public void When_checking_if_an_existing_value_exists_return_true()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(11, t);
            Assert.IsTrue(_dict.ContainsValue(t));
        }

        [Test]
        public void When_checking_if_a_nonexisting_value_exists_return_false()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(1, t);
            TestFixture t2 = new TestFixture(11);
            Assert.IsFalse(_dict.ContainsValue(t2));
        }

        [Test]
        public void When_trying_to_get_an_existing_value_return_the_value()
        {
            TestFixture t = new TestFixture(10);
            _dict.Add(1, t);
            TestFixture actual;
            _dict.TryGetValue(1, out actual);
            Assert.AreEqual(t, actual);
        }

        [Test]
        public void When_overwriting_an_existing_value_return_the_correct_value()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);
            _dict[1] = t;
            _dict[1] = t2;
            TestFixture actual;
            _dict.TryGetValue(1, out actual);
            Assert.AreEqual(t2, actual);
        }

        [Test]
        public void When_using_copyto_verify_the_result()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);

            _dict[12] = t;
            _dict[15] = t2;
            KeyValuePair<int, TestFixture>[] array = new KeyValuePair<int, TestFixture>[4];
            _dict.CopyTo(array, 2);
            Assert.AreEqual(12, array[2].Key);
            Assert.AreEqual(t, array[2].Value);

            Assert.AreEqual(15, array[3].Key);
            Assert.AreEqual(t2, array[3].Value);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void When_copyto_a_null_array_throw_exception()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);
            _dict[12] = t;
            _dict[15] = t2;
            KeyValuePair<int, TestFixture>[] array = null;
            _dict.CopyTo(array, 0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void When_copyto_and_index_is_outside_array_size_throw_exception()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);
            _dict[12] = t;
            _dict[15] = t2;
            KeyValuePair<int, TestFixture>[] array = new KeyValuePair<int, TestFixture>[10];
            _dict.CopyTo(array, 10);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void When_copyto_and_index_is_negative_throw_exception()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);
            _dict[12] = t;
            _dict[15] = t2;
            KeyValuePair<int, TestFixture>[] array = new KeyValuePair<int, TestFixture>[10];
            _dict.CopyTo(array, -1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void When_copyto_and_array_is_smaller_than_dictionary_throw_exception()
        {
            TestFixture t = new TestFixture(10);
            TestFixture t2 = new TestFixture(11);
            _dict[12] = t;
            _dict[15] = t2;
            KeyValuePair<int, TestFixture>[] array = new KeyValuePair<int, TestFixture>[1];
            _dict.CopyTo(array, 0);
        }

        [Test]
        public void When_adding_several_known_sized_items_verify_that_they_exist()
        {
            for (int i = 0; i < 100; i++)
            {
                _dict[i] = new TestFixture(i + 20);
            }

            for (int i = 0; i < 100; i++)
            {
                Assert.AreEqual(new TestFixture(i + 20), _dict[i]);
            }
        }

        [Test]
        public void When_iteration_over_the_dictionary_throw_exception_if_items_are_removed()
        {
            _dict[0] = new TestFixture(0);
            _dict[1] = new TestFixture(1);
            var enumerator = _dict.GetEnumerator();
            enumerator.MoveNext();
            _dict.Remove(0);
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }

        [Test]
        public void When_iteration_over_the_dictionary_throw_exception_if_items_are_added_by_accessor()
        {
            _dict[0] = new TestFixture(0);
            _dict[1] = new TestFixture(1);
            var enumerator = _dict.GetEnumerator();
            enumerator.MoveNext();
            _dict[2] = new TestFixture(2);
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }

        [Test]
        public void When_iteration_over_the_dictionary_throw_exception_if_items_are_added()
        {
            _dict[0] = new TestFixture(0);
            _dict[1] = new TestFixture(1);
            var enumerator = _dict.GetEnumerator();
            enumerator.MoveNext();
            _dict.Add(2, new TestFixture(2));
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }
    }
}
