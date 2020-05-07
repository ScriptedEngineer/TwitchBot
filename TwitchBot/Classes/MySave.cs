using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;


namespace TwitchBot
{
    public class MySave
    {
        const int BL = 9;
        const int NL = 5;

        public static MySave Current = new MySave();
        public string Streamer { get; set; }
        public string TTSCRID { get; set; }
        public string TTSNTFL { get; set; }
        public string TTSCRTitle { get; set; }
        public string BadWords { get; set; }
        public string Censor { get; set; }
        public string OBSWSPort { get; set; }
        public string OBSWSPass { get; set; }
        private static Random Rand = new Random();
        public bool[] Bools { get; set; }
        public int[] Nums { get; set; }
        public string[] Strings { get; set; }
        public KeyModifier HotkeyModifier = KeyModifier.Alt;
        public Key Hotkey = Key.F1;
        private MySave()
        {
            Bools = new bool[BL];
            Nums = new int[NL];
            Nums[0] = 0;
            Nums[4] = 100;
            OBSWSPort = "4444";
            //TTSNTFL = $"{Path.GetDirectoryName(Extentions.AppFile)}/tts.mp3";
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
                if (Current.Nums.Length < NL)
                {
                    int[] nms = Current.Nums;
                    Array.Resize<int>(ref nms, NL);
                    Current.Nums = nms;
                }
                if (Current.Bools.Length < BL)
                {
                    bool[] nms = Current.Bools;
                    Array.Resize<bool>(ref nms, BL);
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
