using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Summary description for MemoryMappedDictionaryTest
    /// </summary>
    [TestFixture]
    public class MemoryMappedDictionaryTestLoadExistingFile
    {
        string _path = AppDomain.CurrentDomain.BaseDirectory;
        private bool _error;
        private string _errorMessage;

        [SetUp]
        public void SetUp()
        {
            foreach (var file in Directory.GetFiles(_path, "test1.*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
            foreach (var file in Directory.GetFiles(_path, "mmf*.*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
        }

        [Test]
        public void When_opening_an_existing_file_validate_the_content()
        {
            Thread t = new Thread(CreateDict);
            t.Start();
            t.Join();
            Assert.IsFalse(_error, _errorMessage);

            Thread t2 = new Thread(LoadExistingDict);
            t2.Start();
            t2.Join();
            Assert.IsFalse(_error, _errorMessage);

            Thread.Sleep(4000);            
        }

        [Test]
        public void When_adding_more_items_to_an_existing_file_validate_the_content()
        {
            var dict = new Dictionary<int, int>(_path, 20, true, "test1");
            dict[0] = 0;
            dict[1] = 1;
            dict = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Thread.Sleep(4000);
            dict = new Dictionary<int, int>(_path, 20, false, "test1");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(0, dict[0]);
            Assert.AreEqual(1, dict[1]);
            dict[0] = 2;
            dict[5] = 5;
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(2, dict[0]);
            Assert.AreEqual(5, dict[5]);
        }

        [Test]
        public void When_adding_more_items_to_an_existing_file_validate_the_content2()
        {
            var dict = new Dictionary<string, int>(_path, 20, true, "test1");
            dict["new0"] = 0;
            dict["new1"] = 1;

            foreach (var kvp in dict)
            {
                if( kvp.Key != "new0" && kvp.Key != "new1")
                    Assert.Fail("Error in reading keys");
            }

            dict.Dispose();
            dict = new Dictionary<string, int>(_path, 20, true, "test1");
            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(0, dict["new0"]);
            Assert.AreEqual(1, dict["new1"]);
            dict["test0"] = 2;

            foreach (var kvp in dict)
            {
                if (kvp.Key != "new0" && kvp.Key != "new1" && kvp.Key != "test0")
                    Assert.Fail("Error in reading keys");
            }

            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(0, dict["new0"]);
            Assert.AreEqual(1, dict["new1"]);
            Assert.AreEqual(2, dict["test0"]);

            dict.Dispose();
            dict = new Dictionary<string, int>(_path, 20, true, "test1");
            Assert.AreEqual(3, dict.Count);
            Assert.AreEqual(0, dict["new0"]);
            Assert.AreEqual(1, dict["new1"]);
            Assert.AreEqual(2, dict["test0"]);
        }

        [Test]
        public void When_adding_more_items_to_an_existing_file_validate_the_content3()
        {
            using (var dict = new Dictionary<string, int>(_path, 20, true, "test1"))
            {
                dict["new0"] = 0;
                dict["new1"] = 1;

                foreach (var kvp in dict)
                {
                    if (kvp.Key != "new0" && kvp.Key != "new1")
                        Assert.Fail("Error in reading keys");
                }
            }
            using (var dict = new Dictionary<string, int>(_path, 20, true, "test1"))
            {
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual(0, dict["new0"]);
                Assert.AreEqual(1, dict["new1"]);
                dict["test0"] = 2;

                foreach (var kvp in dict)
                {
                    if (kvp.Key != "new0" && kvp.Key != "new1" && kvp.Key != "test0")
                        Assert.Fail("Error in reading keys");
                }

                Assert.AreEqual(3, dict.Count);
                Assert.AreEqual(0, dict["new0"]);
                Assert.AreEqual(1, dict["new1"]);
                Assert.AreEqual(2, dict["test0"]);
            }

            using (var dict = new Dictionary<string, int>(_path, 20, true, "test1"))
            {
                Assert.AreEqual(3, dict.Count);
                Assert.AreEqual(0, dict["new0"]);
                Assert.AreEqual(1, dict["new1"]);
                Assert.AreEqual(2, dict["test0"]);
            }
        }

        public void CreateDict()
        {
            try
            {
                var dict = new Dictionary<int, int>(_path, 20, true, "test1");
                dict[0] = 0;
                dict[1] = 1;
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual(0, dict[0]);
                Assert.AreEqual(1, dict[1]);
                dict.Dispose();
            }
            catch (AssertionException e)
            {
                //Assertion failed..
                _errorMessage = e.Message;
                _error = true;
            }
        }

        public void LoadExistingDict()
        {
            try
            {
                var dict = new Dictionary<int, int>(_path, 20, true, "test1");
                Assert.AreEqual(2, dict.Count);
                Assert.AreEqual(0, dict[0]);
                Assert.AreEqual(1, dict[1]);
                dict.Dispose();
            }
            catch (AssertionException e)
            {
                //assertfailed
                _errorMessage = e.Message;
                _error = true;
            }
        }
    }
}