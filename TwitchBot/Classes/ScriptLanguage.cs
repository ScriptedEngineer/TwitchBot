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
                    case "Speech":
                        {
                            lock (Extentions.SpeechSynth)
                            {
                                Extentions.TextToSpeech(Regex.Replace(command, "^Speech ", " "));
                            }
                        }
                        break;
                    case "Wait":
                        {
                            if (byte.TryParse(param[1], out byte Kb))
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
    }
}
