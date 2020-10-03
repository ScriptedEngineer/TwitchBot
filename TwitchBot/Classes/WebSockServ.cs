using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TwitchBot
{
    public class WebSockServ : WebSocketBehavior
    {
        static WebSockServ Connection;
        public WebSockServ()
        {
            Connection = this;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Ping")
                Send("Pong");
            else
            {
                string[] Par = e.Data.Split('|');
                switch (Par[0])
                {
                    case "timer.End":
                        if (Par.Length > 1)
                        {
                            if (ScriptLanguage.TimersEnds.ContainsKey(Par[1]))
                            {
                                ScriptLanguage.RunCommand(ScriptLanguage.TimersEnds[Par[1]]);
                                ScriptLanguage.TimersEnds.Remove(Par[1]);
                            }
                        }
                        break;
                }
            }
        }

        public static void SendAll(string Event, string Data = "")
        {
            Connection?.Sessions.Broadcast(String.Format("{0}|{1}", Event, Data));
        }
        public static void SendAll(string Event, params string[] Data)
        {
            Connection?.Sessions.Broadcast(String.Format("{0}|{1}", Event, string.Join("|", Data)));
        }
    }
    public class WebSockServKeybd : WebSocketBehavior
    {
        static WebSockServKeybd Connection;
        static Thread Hook;
        static int MouseDelta = 0;
        static byte MouseDelted = 41;
        static readonly Dictionary<byte, bool> KeyStates = new Dictionary<byte, bool>();
        public WebSockServKeybd()
        {
            Connection = this;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Ping")
                Send("Pong");
        }

        public static void SendAll(string Event, string Key)
        {
            Connection?.Sessions.Broadcast(String.Format("{0}|{1}", Event, Key));
        }
        public static void Init(bool Enable)
        {
            if (Enable)
            {
                if (Hook == null)
                {
                    WinApi.MouseHook.MouseAction += MouseHook_MouseAction;
                    WinApi.MouseHook.Start();
                    Task.Run(() =>
                    {
                        Hook = Thread.CurrentThread;
                        KeyStates.Clear();
                        while (true)
                        {
                            Thread.Sleep(1);
                            if (MouseDelta != 0)
                            {
                                if (MouseDelted >= 41) SendAll("Press", "4");
                                MouseDelta = 0;
                                MouseDelted = 0;

                            }
                            else if (MouseDelted < 40)
                            {
                                MouseDelted++;
                            }
                            else if (MouseDelted == 40)
                            {
                                MouseDelted++;
                                SendAll("Unpress", "4");
                            }
                            for (byte i = 0; i < 255; i++)
                            {
                                bool presed = WinApi.GetKeyState(i);
                                if (!KeyStates.ContainsKey(i))
                                {
                                    KeyStates.Add(i, false);
                                    continue;
                                }
                                if (KeyStates[i] != presed)
                                {
                                    SendAll(presed ? "Active" : "Deactive", i.ToString());
                                    KeyStates[i] = presed;
                                }
                            }
                        }
                    });
                }
            }
            else
            {
                if (Hook != null) {
                    WinApi.MouseHook.MouseAction -= MouseHook_MouseAction;
                    WinApi.MouseHook.stop();
                    Hook.Abort();
                    Hook = null;
                }
            }
        }

        static private void MouseHook_MouseAction(object sender, WinApi.MouseHook.MouseEventArgs e)
        {
            MouseDelta += e.Wheel;
            /*WinApi.MousePoint x = new WinApi.MousePoint(e.X, e.Y);
            Console.WriteLine(e.X+"|"+ e.Y);
            if (MousePos != x)
            {
                InputInstructions.Append($"{st.ElapsedMilliseconds} Mouse Set {e.X} {e.Y}\n");
                MousePos = x;
                st.Restart();
            }
            //Console.WriteLine(e.Wheel);*/
        }

    }

}
