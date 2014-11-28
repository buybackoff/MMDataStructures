using System;
using MMDataStructures;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    ///This is a test class for MemoryMappedListTest and is intended
    ///to contain all MemoryMappedListTest Unit Tests
    ///</summary>
    [TestFixture]
    public class MemoryMappedListTest
    {
        private List<int> _testList;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        

        [SetUp]
        public void MyTestInitialize()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            _testList = new List<int>(1024 * 1024, path);
        }

        [TearDown]
        public void MyTestCleanup()
        {
            _testList.Dispose();
        }

        [Test]
        public void When_list_is_initialized_it_contains_zero_items()
        {
            int expected = 0;
            int actual = _testList.Count;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void When_one_item_is_added_the_list_contains_one_item()
        {
            _testList.Add(42);

            int expected = 1;
            int actual = _testList.Count;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void When_one_item_is_added_the_same_item_can_be_retrieved()
        {
            _testList.Add(42);
            int expected = 42;
            int actual = _testList[0];
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void When_accessing_an_index_outside_bounds_an_exception_is_thrown()
        {
            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { _testList[-1] = 42; });
            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { int a = _testList[-1]; });

            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { _testList[10] = 42; });
            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { int a = _testList[10]; });
        }

        [Test]
        public void When_an_item_is_overwritten_we_get_the_correct_value_back()
        {
            _testList.Add(42);
            _testList.Add(43);
            _testList.Add(44);
            int expected = 53;
            _testList[1] = expected;
            int actual = _testList[1];
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void When_clearing_the_list_no_items_are_accessible()
        {
            _testList.Add(1);
            _testList.Add(2);
            Assert.AreEqual(2, _testList.Count);
            _testList.Clear();
            Assert.AreEqual(0, _testList.Count);

            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { _testList[0] = 42; });
            MyAssert.ThrowsException<ArgumentOutOfRangeException>(() => { int a = _testList[0]; });
        }

        [Test]
        public void When_inserting_an_item_existing_items_are_moved_and_list_is_increased_by_one()
        {
            _testList.Add(1);
            _testList.Add(2);
            Assert.AreEqual(2, _testList.Count);

            _testList.Insert(0, 0);
            Assert.AreEqual(0, _testList[0]);
            Assert.AreEqual(1, _testList[1]);
            Assert.AreEqual(2, _testList[2]);
            Assert.AreEqual(3, _testList.Count);
        }

        [Test]
        public void When_removing_an_item_existing_items_are_moved_and_list_is_decreased_by_one()
        {
            _testList.Add(1);
            _testList.Add(2);
            _testList.Add(3); // 1,2,3
            Assert.AreEqual(3, _testList.Count);

            _testList.RemoveAt(1); // 1,3
            Assert.AreEqual(1, _testList[0]);
            Assert.AreEqual(3, _testList[1]);
            Assert.AreEqual(2, _testList.Count);

            _testList.Add(42); // 1,3,42
            Assert.AreEqual(1, _testList[0]);
            Assert.AreEqual(3, _testList[1]);
            Assert.AreEqual(42, _testList[2]);

            Assert.IsTrue(_testList.Remove(1)); // 3,42
            Assert.AreEqual(3, _testList[0]);
            Assert.AreEqual(42, _testList[1]);
            Assert.AreEqual(2, _testList.Count);
        }

        [Test]
        public void When_adding_items_to_a_list_they_are_verified_to_be_in_the_list()
        {
            _testList.Add(1);
            _testList.Add(2);
            _testList.Add(3);
            _testList.Add(4);
            _testList.Add(5);
            Assert.IsTrue(_testList.Contains(1));
            Assert.IsTrue(_testList.Contains(2));
            Assert.IsTrue(_testList.Contains(3));
            Assert.IsTrue(_testList.Contains(4));
            Assert.IsTrue(_testList.Contains(5));

            Assert.AreEqual(0, _testList.IndexOf(1));
            Assert.AreEqual(1, _testList.IndexOf(2));
            Assert.AreEqual(2, _testList.IndexOf(3));
            Assert.AreEqual(3, _testList.IndexOf(4));
            Assert.AreEqual(4, _testList.IndexOf(5));
        }

        [Test]
        public void Length_and_count_returns_the_same_value()
        {
            Assert.AreEqual(0, _testList.Length);
            Assert.AreEqual(_testList.Count, _testList.Length);

            _testList.Add(2);

            Assert.AreEqual(1, _testList.Length);
            Assert.AreEqual(_testList.Count, _testList.Length);

            _testList.Remove(2);

            Assert.AreEqual(0, _testList.Length);
            Assert.AreEqual(_testList.Count, _testList.Length);
        }

        [Test]
        public void When_copying_items_from_the_list_to_the_array_the_items_are_the_same()
        {
            for (int i = 0; i < 2000; i++)
            {
                _testList.Add(i);
            }

            Assert.AreEqual(2000, _testList.Length);

            int[] array = new int[3000];
            _testList.CopyTo(array, 500);
            for (int i = 500; i < 2500; i++)
            {
                Assert.AreEqual(array[i], _testList[i - 500]);
            }
        }

        [Test]
        public void When_iterating_over_the_list_the_items_are_the_same()
        {
            for (int i = 0; i < 2000; i++)
            {
                _testList.Add(i);
            }
            Assert.AreEqual(2000, _testList.Count);
            int expected = 0;
            foreach (int i in _testList)
            {
                Assert.AreEqual(expected, i);
                expected++;
            }
        }

        [Test]
        public void When_iteration_over_the_list_throw_exception_if_items_are_removed()
        {
            _testList.Add(1);
            _testList.Add(2);
            var enumerator = _testList.GetEnumerator();
            enumerator.MoveNext();
            _testList.Remove(1);
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }

        [Test]
        public void When_iteration_over_the_list_throw_exception_if_items_are_added()
        {
            _testList.Add(1);
            _testList.Add(2);
            var enumerator = _testList.GetEnumerator();
            enumerator.MoveNext();
            _testList.Add(1);
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }

        [Test]
        public void When_iteration_over_the_list_throw_exception_if_items_are_inserted()
        {
            _testList.Add(1);
            _testList.Add(2);
            var enumerator = _testList.GetEnumerator();
            enumerator.MoveNext();
            _testList.Insert(0,5);
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }
    }
}