using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TwitchBot
{
    public class TwitchAccount
    {
        public static TwitchAccount Current = new TwitchAccount();
        public string login, token, moderToken, timeoutSHA, deleteSHA;
        private TwitchAccount()
        {
            login = "justinfan99999";
            token = "SCHMOOPIIE";
            moderToken = "None";
            timeoutSHA = "None";
            deleteSHA = "None";
        }
        public static void Load()
        {
            if (File.Exists("account.xml"))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(TwitchAccount));
                using (FileStream fs = new FileStream("account.xml", FileMode.OpenOrCreate))
                {
                    Current = (TwitchAccount)formatter.Deserialize(fs);
                }
            }
            else
            {
                Save();
            }
        }
        private static void Save()
        {
            XmlSerializer formatter = new XmlSerializer(typeof(TwitchAccount));
            using (FileStream fs = new FileStream("account.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Current);
            }
        }
    }
}
