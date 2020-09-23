using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TwitchBot
{
    public class WebSockServAlert : WebSocketBehavior
    {
        static WebSockServAlert Connection;
        public WebSockServAlert()
        {
            Connection = this;
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Ping")
                Send("Pong");
        }

        public static void SendAll(string Event,string Data = "")
        {
            Connection?.Sessions.Broadcast(String.Format("{0}|{1}", Event, Data));
        }
    }
    public class WebSockServTimer : WebSocketBehavior
    {
        static WebSockServTimer Connection;
        public WebSockServTimer()
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
                    case "TimerEnd":
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

        public static void SendAll(string StringTime, string Title)
        {
            Connection?.Sessions.Broadcast(String.Format("Add|{0}|{1}", StringTime, Title));
        }
    }

}
