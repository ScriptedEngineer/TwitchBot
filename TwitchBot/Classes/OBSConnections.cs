using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace TwitchBot
{
    class OBSWebSock
    {
        static WebSocket WSock = null;
        static int MSGID = 0;
        public OBSWebSock()
        {
            while (!(WSock != null && WSock.IsAlive))
            {
                WSock = new WebSocket($"ws://localhost:{MySave.Current.OBSWSPort}/");
                WSock.OnMessage += WSock_OnMessage;
                WSock.OnClose += WSock_OnClose;
                WSock.OnOpen += WSock_OnOpen;
                WSock.Connect();
                Thread.Sleep(1000);
            }
        }

        public static void SetSourceEnabled(string Source, bool Enabled)
        {
            SendPackage("SetSceneItemProperties", new Dictionary<string, string>() {
                {"item",Source},
                {"visible",(Enabled?"true":"false")} 
            });
        }
        public static void SetSourceRotation(string Source, string angle)
        {
            SendPackage("SetSceneItemProperties", new Dictionary<string, string>() {
                {"item",Source},
                {"rotation",angle}
            });
        }
        public static void SetSourcePosition(string Source, string x, string y)
        {
            SendPackage("SetSceneItemProperties", new Dictionary<string, string>() {
                {"item",Source},
                {"position","{\"x\":"+x+",\"y\":"+y+"}"}
            });
        }
        public static void SetScene(string Scene)
        {
            SendPackage("SetCurrentScene", new Dictionary<string, string>() {
                {"scene-name",Scene}
            });
        }
        public static void SetSourceMute(string Source, bool Enabled)
        {
            SendPackage("SetMute", new Dictionary<string, string>() {
                {"source",Source},
                {"mute",(Enabled?"true":"false")}
            });
        }
        public static void SendTransition()
        {
            SendPackage("TransitionToProgram", new Dictionary<string, string>());
        }

        public static void ReAuth()
        {
            if (WSock == null || !WSock.IsAlive)
                return;
            MSGID++;
            WSock.Send(@"{""request-type"":""GetAuthRequired"",""message-id"":""-2""}");
        }

        private static void SendPackage(string Type,Dictionary<string,string> parames)
        {
            if (!WSock.IsAlive) 
                return;
            string parameters = "";
            foreach(var pp in parames)
            {
                bool isstring = true;
                if (isstring == true) isstring = !int.TryParse(pp.Value, out int n1);
                if (isstring == true) isstring = !bool.TryParse(pp.Value, out bool n2);
                if (isstring == true) isstring = !pp.Value.StartsWith("{");
                parameters += $"\"{pp.Key}\":"+(isstring?$"\"{pp.Value}\",":$"{pp.Value},");
            }
            MSGID++;
            WSock.Send(@"{""request-type"":"""+ Type + @"""," + parameters + @"""message-id"":""" + MSGID + @"""}");
        }
        private void WSock_OnOpen(object sender, EventArgs e)
        {
            MSGID++;
            WSock.Send(@"{""request-type"":""GetAuthRequired"",""message-id"":"""+ MSGID + @"""}");
        }
        private void WSock_OnClose(object sender, CloseEventArgs e)
        {
            Extentions.AsyncWorker(() =>
            {
                if (MainWindow.CurrentW.OBSRstatus.Content.ToString() == "OBS Подключен")
                    MainWindow.CurrentW.OBSRstatus.Content = "Соединение с OBS было разорвано";
                else
                    MainWindow.CurrentW.OBSRstatus.Content = "Не удалось подключится к OBS";
            });
            Thread.Sleep(1000);
            Extentions.AsyncWorker(() => {
                    MainWindow.CurrentW.OBSRstatus.Content = "Переподключение через 3...";
            });
            Thread.Sleep(1000);
            Extentions.AsyncWorker(() => {
                MainWindow.CurrentW.OBSRstatus.Content = "Переподключение через 2...";
            });
            Thread.Sleep(1000);
            Extentions.AsyncWorker(() => {
                MainWindow.CurrentW.OBSRstatus.Content = "Переподключение через 1...";
            });
            Thread.Sleep(1000);
            Extentions.AsyncWorker(() => {
                MainWindow.CurrentW.OBSRstatus.Content = "Подключение...";
            });
            while (!(WSock != null && WSock.IsAlive))
            {
                WSock = new WebSocket($"ws://localhost:{MySave.Current.OBSWSPort}/");
                WSock.OnMessage += WSock_OnMessage;
                WSock.OnClose += WSock_OnClose;
                WSock.OnOpen += WSock_OnOpen;
                WSock.Connect();
                Thread.Sleep(1000);
            }
        }
        private void WSock_OnMessage(object sender, MessageEventArgs e)
        {
            Match RGX = Regex.Match(e.Data, @"\""authRequired\""\:\s(\w*),");
            if (RGX.Success)
            {
                bool.TryParse(RGX.Groups[1]?.Value, out bool xds);
                if (xds)
                {
                    string challenge = Regex.Match(e.Data, @"\""challenge\""\:\s\""([^""]*)\""").Groups[1]?.Value;
                    string salt = Regex.Match(e.Data, @"\""salt\""\:\s\""([^""]*)\""").Groups[1]?.Value;
                    using (SHA256Managed sha = new SHA256Managed())
                    {
                        string saltedhashedpass = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(MySave.Current.OBSWSPass + salt)));
                        string finaly = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(saltedhashedpass + challenge)));
                        MSGID++;
                        WSock.Send(@"{""request-type"":""Authenticate"",""auth"":""" + finaly + @""",""message-id"":""-1""}");
                    }
                }
                MSGID++;
                WSock.Send(@"{""request-type"":""SetHeartbeat"",""enable"":false,""message-id"":""" + MSGID + @"""}");
                //Console.WriteLine(xds.ToString());

            }
            else
            {
                Match sdgk = Regex.Match(e.Data, @"\""error\""\:\s\""([^""]*)\""");
                if (sdgk.Success)
                {
                    string msghf = sdgk.Groups[1]?.Value;
                    if (!msghf.Contains("already"))
                    Extentions.AsyncWorker(() =>
                    MainWindow.CurrentW.OBSRstatus.Content = "Проблема с OBS("+ msghf + ")");
                }
                else
                {
                    Match sdgk2 = Regex.Match(e.Data, @"\""message-id\""\:\s\""([^""]*)\""");
                    Match sdgk1 = Regex.Match(e.Data, @"\""status\""\:\s\""([^""]*)\""");
                    if (sdgk2.Groups[1]?.Value == "-1" && sdgk1.Groups[1]?.Value == "ok")
                    {
                        Extentions.AsyncWorker(() =>
                           MainWindow.CurrentW.OBSRstatus.Content = "OBS Подключен");
                    }
                }
            }
            /*else
            {
                Match HBT = Regex.Match(e.Data, @"\""update-type\""\:\s\""([^""]*)\""");
                if (HBT.Success && HBT.Groups[1]?.Value == "Heartbeat")
                {
                    CurrentScene = Regex.Match(e.Data, @"\""current-scene\""\:\s\""([^""]*)\""").Groups[1]?.Value;
                }
            }
            Console.WriteLine(e.Data);*/
        }
    }
}
