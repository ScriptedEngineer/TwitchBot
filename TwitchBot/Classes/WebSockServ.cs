using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        public static void SendAll(string Event,string Data = "")
        {
            Connection?.Sessions.Broadcast(String.Format("{0}|{1}", Event, Data));
        }
    }

}
