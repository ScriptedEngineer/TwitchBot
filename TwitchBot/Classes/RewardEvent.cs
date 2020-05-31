using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private bool Runing;
        public RewardEvent()
        {

        }
        public void invoke(RewardEventArgs e)
        {
            while (Runing)
                Thread.Sleep(100);
            Runing = true;
            string scripd = Script;
            if (scripd.Contains("%"))
            {

                if (scripd.Contains("%TEXT%"))
                    scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", "").Trim());
                if (scripd.Contains("%NICK%"))
                    scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", "").Trim());
                if (scripd.Contains("%TITLE%"))
                    scripd = scripd.Replace("%TITLE%", e.Title.Replace("\n", "").Trim());
                //if (scripd.Contains("%TEXT%"))
                //  scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
            }
            ScriptLanguage.RunScript(scripd);
            Runing = false;
        }
    }
    public class ComandEvent
    {
        public string Comand = "тост",
            Script;
        public UserRights Right = UserRights.All;
        private bool Runing;
        public ComandEvent()
        {

        }
        public void invoke(MessageEventArgs e)
        {
            while (Runing)
                Thread.Sleep(100);
            Runing = true;
            string scripd = Script;
            if (scripd.Contains("%"))
            {

                if (scripd.Contains("%TEXT%"))
                    scripd = scripd.Replace("%TEXT%", e.Message.Replace("\n", "").Replace(">", "").Split(new char[] { ' ' }, 2).Last().Trim());
                if (scripd.Contains("%NICK%"))
                    scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", "").Trim());
                //if (scripd.Contains("%TEXT%"))
                //  scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
            }
            ScriptLanguage.RunScript(scripd);
            Runing = false;
        }
    }
}
