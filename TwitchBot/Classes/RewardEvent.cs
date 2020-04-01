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
        public EventTypes Type = EventTypes.InputEmu;
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
                    if (scripd.Contains("%TEXT%"))
                        scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
                }
                switch (Type)
                {
                    case EventTypes.InputEmu:
                        ScriptLanguage.RunScript(scripd);
                        break;
                    case EventTypes.Console:
                        Process cmd = new Process();
                        cmd.StartInfo.FileName = "cmd.exe";
                        cmd.StartInfo.RedirectStandardInput = true;
                        cmd.StartInfo.RedirectStandardOutput = true;
                        cmd.StartInfo.CreateNoWindow = true;
                        cmd.StartInfo.UseShellExecute = false;
                        cmd.Start();

                        cmd.StandardInput.WriteLine(scripd);
                        cmd.StandardInput.Flush();
                        cmd.StandardInput.Close();
                        cmd.WaitForExit();
                        break;
                    case EventTypes.Script:
                        MessageBox.Show("В разработке");
                        break;
                }
            }
        }
    }
    public enum EventTypes
    {
        InputEmu,
        Console,
        Script
    }
}
