using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMDataStructures
{

    public static class Config
    {
        static Config()
        {
            Serializer = new DefaultSerializer();
            DataPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
        }
        public static ISerializer Serializer { get; set; }
        /// <summary>
        /// Path where data files are stored
        /// </summary>
        public static string DataPath { get; set; }
    }
}
