using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TwitchBot
{
    public class MySave
    {
        public static MySave Current = new MySave();
        public string Streamer { get; set; }
        public string TTSCRID { get; set; }
        private static Random Rand = new Random();
        public bool[] Bools { get; set; }
        public int[] Nums { get; set; }
        private MySave()
        {
            Bools = new bool[4];
            Nums = new int[2];
            Nums[0] = 0;
        }
        public static void Load()
        {
            if (File.Exists("save.xml"))
            {
                try
                {
                    XmlSerializer formatter = new XmlSerializer(typeof(MySave));
                    using (FileStream fs = new FileStream("save.xml", FileMode.OpenOrCreate))
                    {
                        Current = (MySave)formatter.Deserialize(fs);
                    }
                }
                catch
                {
                    File.Copy("save.xml", "save_errored"+ Rand.Next()+ ".xml");
                    Current = new MySave();
                    Save();
                }
                if (Current.Nums.Length != 2)
                {
                    int[] nms = Current.Nums;
                    Array.Resize<int>(ref nms, 2);
                    Current.Nums = nms;
                }
                if (Current.Nums.Length != 2)
                {
                    bool[] nms = Current.Bools;
                    Array.Resize<bool>(ref nms, 2);
                    Current.Bools = nms;
                }
            }
        }
        public static void Save()
        {
            MySave X = Current;
            XmlSerializer formatter = new XmlSerializer(typeof(MySave));
            if(File.Exists("save.xml"))File.Delete("save.xml");
            using (FileStream fs = new FileStream("save.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, X);
            }
        }
    }
}
