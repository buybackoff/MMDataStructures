using System;
using System.Diagnostics;
using MMDataStructures;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    ///This is a test class for MemoryMappedArrayTest and is intended
    ///to contain all MemoryMappedArrayTest Unit Tests
    ///</summary>
    [TestFixture]
    public class MemoryMappedArrayTest
    {
        private Array<int> _testList;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        

        [TearDown]
        public void MyTestCleanup()
        {
            _testList.Dispose();
        }

        [SetUp]
        public void TestInit()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            _testList = new Array<int>(10, path);
        }

        [Test]
        public void Set_and_get_a_value_within_defined_range()
        {
            long position = (_testList.Length - 1000) >= 0 ? _testList.Length : 0;
            const int num = 234;
            _testList[position] = num;
            Assert.AreEqual(_testList[position], num);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Set_a_value_outside_defined_range_and_autogrow_is_false()
        {
            long position = _testList.Length + 1000;
            const int num = 234;
            _testList[position] = num;
        }

        [Test]
        public void Set_and_get_a_value_outside_defined_range_and_autogrow_is_true()
        {
            _testList.AutoGrow = true;
            long position = _testList.Length + 1000;
            const int num = 234;
            _testList[position] = num;
            Assert.AreEqual(_testList[position], num);
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Set_a_negative_index_value()
        {
            _testList[-1] = 1;
        }

        [Test]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Access_a_negative_index_value()
        {
            int test = _testList[-1];
        }

        [Test]
        public void Iterate_over_all_values()
        {
            for (int i = 0; i < _testList.Length; i++)
            {
                _testList[i] = i;
            }            
            int expected = 0;
            foreach (int i in _testList)
            {
                Assert.AreEqual(i, expected);
                expected++;
            }
        }

        [Test]
        public void When_iteration_over_the_array_throw_exception_if_array_is_changed()
        {
            _testList[0] = 0;
            _testList[1] = 1;
            var enumerator = _testList.GetEnumerator();
            enumerator.MoveNext();
            _testList[1] = 3;
            MyAssert.ThrowsException<InvalidOperationException>(() => { enumerator.MoveNext(); });
        }

        //[Test]
        //public void Sparse()
        //{
        //    _testList.AutoGrow = true;

        //    Random r = new Random(); ;
        //    for (int i = 0; i < 20000; i++)
        //    {
        //        if (i % 1000 == 0)
        //        {
        //            Trace.WriteLine(i);
        //        }
        //        long pos = r.Next(500000);
        //        _testList[pos] = 2;
        //        Assert.AreEqual(2, _testList[pos]);
        //    }
        //}
    }
}