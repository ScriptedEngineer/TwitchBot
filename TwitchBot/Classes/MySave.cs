using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;


namespace TwitchBot
{
    public class MySave
    {
        const int BL = 14;
        const int NL = 8;

        public static MySave Current = new MySave();
        public string Streamer { get; set; }
        public string TTSCRID { get; set; }
        public string TTSNTFL { get; set; }
        public string DTTSNTFL { get; set; }
        public string TTSCRTitle { get; set; }
        public string BadWords { get; set; }
        public string Censor { get; set; }
        public string OBSWSPort { get; set; }
        public string OBSWSPass { get; set; }
        public YVoices YPV { get; set; }
        public YVoices DYPV { get; set; }
        public string YPT { get; set; }
        private static Random Rand = new Random();
        public bool[] Bools { get; set; }
        public int[] Nums { get; set; }
        public string[] Strings { get; set; }
        public string[] UserRightes { get; set; }
        public KeyModifier HotkeyModifier = KeyModifier.Alt;
        public Key Hotkey = Key.F1;
        public static Dictionary<string, UserRights> UsersRights = new Dictionary<string, UserRights>();
        public static Dictionary<string, UserRights> TmpUsersRights = new Dictionary<string, UserRights>();
        private MySave()
        {
            Bools = new bool[BL];
            Nums = new int[NL];
            Nums[0] = 0;
            Nums[4] = 100;
            OBSWSPort = "4444";
            UserRightes = new string[] { };
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
            foreach(string usrigh in Current.UserRightes)
            {
                try
                {
                    string[] Flds = usrigh.Split(':');
                    if (UsersRights.ContainsKey(Flds[0]))
                        continue;
                    UsersRights.Add(Flds[0], (UserRights)Enum.Parse(typeof(UserRights), Flds[1]));
                }
                catch
                {

                }
            }
        }
        public static void Save()
        {
            List<string> frg = new List<string>();
            foreach (var usrigh in UsersRights)
            {
                try
                {
                    frg.Add(usrigh.Key+":"+usrigh.Value.ToString());
                }
                catch
                {

                }
            }
            Current.UserRightes = frg.ToArray();
            MySave X = Current;
            XmlSerializer formatter = new XmlSerializer(typeof(MySave));
            if(File.Exists("save.xml"))File.Delete("save.xml");
            using (FileStream fs = new FileStream("save.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, X);
            }
        }
    }

    public enum YVoices
    {
        alena,
        filipp,
        alyss,
        jane,
        oksana,
        omazh,
        zahar,
        ermil
    }

    [Flags]
    public enum UsersTTS
    {
        All = 0,
        VIP = 1 << 0,
        Mod = 1 << 1,
        Sub = 1 << 2,
    }

    [Flags]
    public enum UserRights
    {
        Зритель     = 0,
        VIP         = 1 << 0,
        Модератор   = 1 << 1,
        Подписчик   = 1 << 8,
        Создатель   = 1 << 2,

        ping        = 1 << 3,
        tts         = 1 << 4,
        speech      = 1 << 5,
        notify      = 1 << 6,
        coin        = 1 << 7,

        All         = ~0

    }
}
