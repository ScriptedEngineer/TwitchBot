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

namespace TwitchBot
{
    class ScriptLanguage
    {
        static public void RunScript(string script)
        {
            string[] functions = script.Split('\n');
            int len = functions.Length;
            for (int index = 0; index < len; index++)
            {
                index = RunCommand(functions[index].Trim(), index, len);
            }
        }

        static public int RunCommand(string command, int index, int max)
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
                        switch (param[1])
                        {
                            case "Add":
                                    if (!MySave.UsersRights.ContainsKey(param[2].ToLower()))
                                        MySave.UsersRights.Add(param[2].ToLower(), (UserRights)Enum.Parse(typeof(UserRights), param[3]));
                                    else
                                        MySave.UsersRights[param[2].ToLower()] |= (UserRights)Enum.Parse(typeof(UserRights), param[3]);
                                break;
                            case "Del":
                                    if (!MySave.UsersRights.ContainsKey(param[2].ToLower()))
                                        MySave.UsersRights.Add(param[2].ToLower(), UserRights.Зритель);
                                    else
                                        MySave.UsersRights[param[2].ToLower()] &= ~(UserRights)Enum.Parse(typeof(UserRights), param[3]);
                                break;
                        }
                        break;
                    case "TmpRights":
                        switch (param[1])
                        {
                            case "Add":
                                if (!MySave.TmpUsersRights.ContainsKey(param[2].ToLower()))
                                    MySave.TmpUsersRights.Add(param[2].ToLower(), (UserRights)Enum.Parse(typeof(UserRights), param[3]));
                                else
                                    MySave.TmpUsersRights[param[2].ToLower()] |= (UserRights)Enum.Parse(typeof(UserRights), param[3]);
                                break;
                            case "Del":
                                if (!MySave.TmpUsersRights.ContainsKey(param[2].ToLower()))
                                    MySave.TmpUsersRights.Add(param[2].ToLower(), UserRights.Зритель);
                                else
                                    MySave.TmpUsersRights[param[2].ToLower()] &= ~(UserRights)Enum.Parse(typeof(UserRights), param[3]);
                                break;
                        }
                        break;
                    case "Speech":
                        lock (Extentions.SpeechSynth)
                        {
                            Extentions.TextToSpeech(Regex.Replace(command, "^Speech ", " ").Trim());
                        }
                        break;
                    case "TTS":
                        lock (Extentions.SpeechSynth)
                        {
                            string Text = Regex.Replace(command, "^TTS ", " ").Trim();
                            Extentions.GetTrueTTSReady(Text, MySave.Current.YPS);
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
                                }
                                break;
                            case "Scene":
                                {
                                    OBSWebSock.SetScene(Regex.Replace(command, "^OBS Scene ", "").Trim());
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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
