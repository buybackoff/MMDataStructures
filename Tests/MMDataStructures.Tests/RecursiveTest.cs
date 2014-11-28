using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MMDataStructures.Test
{
    /// <summary>
    /// Summary description for RecursiveTest
    /// </summary>
    [TestFixture]
    public class RecursiveTest
    {
        public class CSandboxFileInfo
        {
            public string La { get; set; }
            public string Lu { get; set; }
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        


        [Test]
        public void Accessing_collection_while_iterating_should_not_thow_exception()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            Dictionary<string, CSandboxFileInfo> dictSandboxFileInfo = new Dictionary<string, CSandboxFileInfo>(path,
                                                                                                                1000);
            CSandboxFileInfo c = new CSandboxFileInfo {La = "lalalalala", Lu = "lululululu"};
            dictSandboxFileInfo.Add("somekey", c);


            Dictionary<string, string> keyDict = new Dictionary<string, string>(path, 1000);
            KeyValuePair<string, string> kvp = new KeyValuePair<string, string>("somekey", "bbbb");
            keyDict.Add(kvp);

            foreach (KeyValuePair<string, string> deSandboxFile in keyDict)
            {
                var val = dictSandboxFileInfo[deSandboxFile.Key];
                var obj = keyDict[deSandboxFile.Key];
            }
        }
    }
}