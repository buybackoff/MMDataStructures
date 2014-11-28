using System;
using System.Threading;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    [TestFixture]
    public class ThreadingTest
    {
        #region test1

        [Test]
        public void Array_thread_test()
        {
            _error = false;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            using (Array<int> testList = new Array<int>(10, path))
            {
                testList.AutoGrow = true;
                System.Collections.Generic.List<Thread> tList = new System.Collections.Generic.List<Thread>(100);
                for (int i = 0; i < 100; i++)
                {
                    Thread t = new Thread(DoWriteTest1);
                    tList.Add(t);
                    t.Start(testList);
                }

                for (int i = 0; i < 100; i++)
                {
                    Thread t = new Thread(DoReadTest1);
                    tList.Add(t);
                    t.Start(testList);
                }

                foreach (Thread t in tList)
                {
                    t.Join();
                }
                Assert.IsFalse(_error);
            }
        }

        private static bool _error;

        private static void DoWriteTest1(object list)
        {
            try
            {
                Random random = new Random();
                Array<int> intList = (Array<int>) list;
                for (int i = 0; i < 100000; i++)
                {
                    intList[random.Next(1000)] = i;
                }
            }
            catch (Exception)
            {
                _error = true;
                throw;
            }
        }

        private static void DoReadTest1(object list)
        {
            try
            {
                Random random = new Random();
                Array<int> intList = (Array<int>) list;
                for (int i = 0; i < 100000; i++)
                {
                    int x = intList[random.Next(1000)];
                }
            }
            catch (Exception)
            {
                _error = true;
                throw;
            }
        }

        #endregion

        #region test2

        [Test]
        public void List_thread_test()
        {
            _error = false;
            string path = AppDomain.CurrentDomain.BaseDirectory;
            using (List<int> testList = new List<int>(10, path))
            {
                System.Collections.Generic.List<Thread> tList = new System.Collections.Generic.List<Thread>(100);
                for (int i = 0; i < 100; i++)
                {
                    Thread t = new Thread(DoWriteTest2);
                    tList.Add(t);
                    t.Start(testList);
                }

                for (int i = 0; i < 100; i++)
                {
                    Thread t = new Thread(DoReadTest2);
                    tList.Add(t);
                    t.Start(testList);
                }

                foreach (Thread t in tList)
                {
                    t.Join();
                }
                Assert.IsFalse(_error);
            }
        }

        private static void DoWriteTest2(object list)
        {
            try
            {
                Random random = new Random();
                List<int> intList = (List<int>) list;
                for (int i = 0; i < 100000; i++)
                {
                    int pos = random.Next(1000);
                    if (pos >= intList.Count)
                    {
                        intList.Add(random.Next(1000));
                    }
                    else
                    {
                        intList[pos] = i;
                    }
                }
            }
            catch (Exception)
            {
                _error = true;
                throw;
            }
        }

        private static void DoReadTest2(object list)
        {
            try
            {
                Random random = new Random();
                List<int> intList = (List<int>) list;
                for (int i = 0; i < 100000; i++)
                {
                    int pos = random.Next(1000);
                    if (pos >= intList.Count)
                    {
                        intList.Add(random.Next(1000));
                    }
                    else
                    {
                        int x = intList[pos];
                    }
                }
            }
            catch (Exception)
            {
                _error = true;
                throw;
            }
        }

        #endregion

        #region test3

        [Test]
        public void Dictionary_thread_test()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            Dictionary<int, int> testDictionary = new Dictionary<int, int>(path);
            try
            {
                _error = false;

                System.Collections.Generic.List<Thread> tList = new System.Collections.Generic.List<Thread>(100);
                for (int i = 0; i < 20; i++)
                {
                    Thread t = new Thread(DoWriteTest3);
                    tList.Add(t);
                    t.Start(testDictionary);
                }

                for (int i = 0; i < 20; i++)
                {
                    Thread t = new Thread(DoReadTest3);
                    tList.Add(t);
                    t.Start(testDictionary);
                }

                foreach (Thread t in tList)
                {
                    t.Join();
                }
                Assert.IsFalse(_error);
            }
            finally
            {
                testDictionary.Dispose();
            }
        }

        private static void DoWriteTest3(object dictionary)
        {
            try
            {
                Random random = new Random();
                Dictionary<int, int> dictionary1 = (Dictionary<int, int>) dictionary;
                for (int i = 0; i < 10000; i++) //100000
                {
                    dictionary1[random.Next(1000)] = i;
                }
            }
            catch (Exception e)
            {
                _error = true;
                throw;
            }
        }

        private static void DoReadTest3(object dictionary)
        {
            try
            {
                Random random = new Random();
                Dictionary<int, int> dictionary1 = (Dictionary<int, int>) dictionary;
                for (int i = 0; i < 10000; i++) //100000
                {
                    int x;
                    dictionary1.TryGetValue(random.Next(1000), out x);
                }
            }
            catch (Exception e)
            {
                _error = true;
                throw;
            }
        }

        #endregion
    }
}