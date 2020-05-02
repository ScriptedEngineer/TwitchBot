using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TwitchLib;

namespace TwitchBot
{
    public class RewardEvent
    {
        public string CustomRewardID,
            RewardName = "Определено пользователем",
            EventName = "Новый",
            Script;
        public RewardEvent()
        {

        }
        public void invoke(RewardEventArgs e)
        {
            lock (CustomRewardID)
            {
                string scripd = Script;
                if (scripd.Contains("%"))
                {
                    if (scripd.Contains("%TEXT%"))
                        scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
                    if (scripd.Contains("%NICK%"))
                        scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", ""));
                    if (scripd.Contains("%TITLE%"))
                        scripd = scripd.Replace("%TITLE%", e.Title.Replace("\n", ""));
                    //if (scripd.Contains("%TEXT%"))
                    //  scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
                }
                ScriptLanguage.RunScript(scripd);
            }
        }
    }
}
