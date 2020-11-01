using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Speech.Synthesis;

namespace TwitchBot
{
    class ScriptLanguage
    {
        public static Dictionary<string, string> TimersEnds = new Dictionary<string, string>();
        static public void RunScript(string script)
        {
            if (script == null) return;
            string[] functions = script.Split('\n');
            int len = functions.Length;
            for (int index = 0; index < len; index++)
            {
                index = RunCommand(functions[index].Trim(), index, functions);//, len
            }
        }

        public static int RunCommand(string command, int index = 0, string[] script = null)//, int max
        {
            try
            {
                string[] param = command.Split(' ');
                if (param.Count() < 1) return index;
                switch (param[0])
                {
                    case "Mouse":
                        switch (param[1])
                        {
                            case "Down":
                                UserInput.MouseButton ClickR;
                                Enum.TryParse(param[2], out ClickR);
                                UserInput.MouseButtonEvent(ClickR, UserInput.ButtonEvents.Down);
                                break;
                            case "Click":
                                UserInput.MouseButton Click;
                                Enum.TryParse(param[2], out Click);
                                UserInput.MouseClick(Click);
                                break;
                            case "Up":
                                UserInput.MouseButton ClickN;
                                Enum.TryParse(param[2], out ClickN);
                                UserInput.MouseButtonEvent(ClickN, UserInput.ButtonEvents.Up);
                                break;
                            case "Scroll":
                                if (!int.TryParse(param[2], out int Yg)) return index;
                                WinApi.MouseEvent(WinApi.MouseEventFlags.Wheel, Yg);
                                break;
                            case "Move":
                                if (!int.TryParse(param[2], out int X)) return index;
                                if (!int.TryParse(param[3], out int Y)) return index;
                                UserInput.MouseMove(X, Y);
                                break;
                            case "Set":
                                if (!int.TryParse(param[2], out int Xa)) return index;
                                if (!int.TryParse(param[3], out int Ya)) return index;
                                UserInput.SetMouse(Xa, Ya);
                                break;
                        }
                        break;
                    case "Button":
                        switch (param[1])
                        {
                            case "Down":
                                {
                                    WinApi.Vk X = 0;
                                    if (int.TryParse(param[2], out int Kb))
                                        X = (WinApi.Vk)Kb;
                                    else
                                        Enum.TryParse("VK_" + param[3], out X);
                                    UserInput.ButtonEvent(X, UserInput.ButtonEvents.Down);
                                }
                                break;
                            case "Click":
                                {
                                    WinApi.Vk X = 0;
                                    if (int.TryParse(param[2], out int Kb))
                                        X = (WinApi.Vk)Kb;
                                    else
                                        Enum.TryParse("VK_" + param[2], out X);
                                    UserInput.ButtonEvent(X, UserInput.ButtonEvents.Down);
                                    UserInput.ButtonEvent(X, UserInput.ButtonEvents.Up);
                                }
                                break;
                            case "Up":
                                {
                                    WinApi.Vk X = 0;
                                    if (byte.TryParse(param[2], out byte Kb))
                                        X = (WinApi.Vk)Kb;
                                    else
                                        Enum.TryParse("VK_" + param[3], out X);
                                    UserInput.ButtonEvent(X, UserInput.ButtonEvents.Up);
                                }
                                break;
                        }
                        break;
                    case "ChangeLayout":
                        UserInput.ButtonEvent(WinApi.Vk.VK_LMENU, UserInput.ButtonEvents.Down);
                        UserInput.ButtonEvent(WinApi.Vk.VK_LSHIFT, UserInput.ButtonEvents.Down);
                        UserInput.ButtonEvent(WinApi.Vk.VK_LSHIFT, UserInput.ButtonEvents.Up);
                        UserInput.ButtonEvent(WinApi.Vk.VK_LMENU, UserInput.ButtonEvents.Up);
                        break;
                    case "Screen":
                        switch (param[1])
                        {
                            case "On":
                                WinApi.SetEnableScreen(true);
                                break;
                            case "Off":
                                WinApi.SetEnableScreen(false);
                                break;
                        }
                        break;
                    case "Rights":
                        {
                            string nick = param[2].ToLower().Trim();
                            string right = param[3].ToLower().Trim();
                            switch (param[1])
                            {

                                case "Add":
                                    if (!MySave.UsersRights.ContainsKey(nick))
                                        MySave.UsersRights.Add(nick, (UserRights)Enum.Parse(typeof(UserRights), right));
                                    else
                                        MySave.UsersRights[nick] |= (UserRights)Enum.Parse(typeof(UserRights), right);
                                    break;
                                case "Del":
                                    if (!MySave.UsersRights.ContainsKey(nick))
                                        MySave.UsersRights.Add(nick, UserRights.Зритель);
                                    else
                                        MySave.UsersRights[nick] &= ~(UserRights)Enum.Parse(typeof(UserRights), right);
                                    break;
                            }
                        }
                        break;
                    case "TmpRights":
                        {
                            string nick = param[2].ToLower().Trim();
                            string right = param[3].ToLower().Trim();
                            switch (param[1])
                            {

                                case "Add":
                                    if (!MySave.TmpUsersRights.ContainsKey(nick))
                                        MySave.TmpUsersRights.Add(nick, (UserRights)Enum.Parse(typeof(UserRights), right));
                                    else
                                        MySave.TmpUsersRights[nick] |= (UserRights)Enum.Parse(typeof(UserRights), right);
                                    break;
                                case "Del":
                                    if (!MySave.TmpUsersRights.ContainsKey(nick))
                                        MySave.TmpUsersRights.Add(nick, UserRights.Зритель);
                                    else
                                        MySave.TmpUsersRights[nick] &= ~(UserRights)Enum.Parse(typeof(UserRights), right);
                                    break;
                            }
                        }
                        break;
                    case "Speech":
                        lock (Extentions.SpeechSynth)
                        {
                            Extentions.TextToSpeech(Regex.Replace(command, "^Speech ", " ").Trim());
                            while (Extentions.SpeechSynth.State == SynthesizerState.Speaking)
                            {
                                Thread.Sleep(100);
                            }
                        }
                        break;
                    case "TTS":
                        lock (Extentions.SpeechSynth)
                        {
                            string Text = Regex.Replace(command, "^TTS ", " ").Trim();
                            Extentions.GetTrueTTSReady(Text, MySave.Current.YPV.ToString());
                            Extentions.TrueTTS(Text);
                        }
                        break;
                    case "CMD":
                        {
                            RunConsole(Regex.Replace(command, "^CMD ", "").Trim());
                        }
                        break;
                    case "Play":
                        lock (Extentions.SpeechSynth)
                        {
                            Extentions.AsyncWorker(() =>
                            {
                                Extentions.Player.Open(new Uri(Regex.Replace(command, "^Play ", " ").Trim(), UriKind.Absolute));
                                Extentions.Player.Play();
                            });
                            Thread.Sleep(1200);
                            Thread.Sleep(MainWindow.CurrentW.MediaDurationMs);
                        }
                        break;
                    case "OBS":
                        switch (param[1])
                        {
                            case "Source":
                                switch (param[2])
                                {
                                    case "On":
                                        OBSWebSock.SetSourceEnabled(Regex.Replace(command, "^OBS Source On ", "").Trim(), true);
                                        break;
                                    case "Off":
                                        OBSWebSock.SetSourceEnabled(Regex.Replace(command, "^OBS Source Off ", "").Trim(), false);
                                        break;
                                    case "Rotation":
                                        {
                                            string[] rc = Regex.Replace(command, "^OBS Source Rotation ", "").Trim().Split('.');
                                            if (rc.Length >= 2)
                                                OBSWebSock.SetSourceRotation(rc[0].Trim(), rc[1].Trim());
                                        }
                                        break;
                                    case "Position":
                                        {
                                            string[] rc = Regex.Replace(command, "^OBS Source Position ", "").Trim().Split('.');
                                            if (rc.Length >= 3)
                                                OBSWebSock.SetSourcePosition(rc[0].Trim(), rc[1].Trim(), rc[2].Trim());
                                        }
                                        break;
                                    case "Scale":
                                        {
                                            string[] rc = Regex.Replace(command, "^OBS Source Scale ", "").Trim().Split('.');
                                            if (rc.Length >= 3)
                                                OBSWebSock.SetSourceScale(rc[0].Trim(), rc[1].Trim(), rc[2].Trim());
                                        }
                                        break;
                                }
                                break;
                            case "Audio":
                                switch (param[2])
                                {
                                    case "On":
                                        OBSWebSock.SetSourceMute(Regex.Replace(command, "^OBS Audio On ", "").Trim(), true);
                                        break;
                                    case "Off":
                                        OBSWebSock.SetSourceMute(Regex.Replace(command, "^OBS Audio Off ", "").Trim(), false);
                                        break;
                                }
                                break;
                            case "Scene":
                                {
                                    OBSWebSock.SetScene(Regex.Replace(command, "^OBS Scene ", "").Trim());
                                }
                                break;
                            case "Transition":
                                {
                                    OBSWebSock.SendTransition();
                                }
                                break;
                        }
                        break;
                    case "Send":
                        MainWindow.Client.SendMessage(Regex.Replace(command, "^Send ", " ").Trim());
                        break;
                    
                    case "Wait":
                        {
                            if (int.TryParse(param[1], out int Kb))
                                Thread.Sleep(Kb);
                        }
                        break;

                    //Многострочные команды
                    case "Timer":
                        string[] heh = command.Trim().Split(new char[] { ' ' }, 3);
                        if (heh.Length > 2)
                        {
                            Match mtch = Regex.Match(heh[2], @"\s(\d+)$");
                            string count = mtch.Groups.Count > 1 ? mtch.Groups[1].Value:"0";
                            heh[2] = heh[2].ToLower().Replace(count,"").Trim();
                            
                            WebSockServ.SendAll("timer.Add", heh[1], heh[2]);
                            if (script != null)
                            {
                                int.TryParse(count, out int kek);
                                string SubScript = "";
                                for (int i = 0; i < kek; i++)
                                {
                                    index++;
                                    if(index < script.Length) 
                                        SubScript += script[index]+"\n";
                                }
                                if (!TimersEnds.ContainsKey(heh[2]))
                                    TimersEnds.Add(heh[2], SubScript);
                                else
                                    TimersEnds[heh[2]] = SubScript;

                            }
                        }
                        break;
                    case "Random":
                        if(int.TryParse(param[1],out int rnd) && script != null)
                        {
                            int random = Extentions.TrueRandom(1, rnd)+ index;
                            RunCommand(script[random]);
                            index += rnd;
                        }
                        break;
                }
            }
            catch //(Exception e)
            {
                //Console.WriteLine(e.Message);
            }
            return index;
        }

        private static void RunConsole(string scripd)
        {
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
        }
    }
}
