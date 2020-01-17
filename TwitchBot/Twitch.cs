using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace TwitchBot
{
    class Twitch
    {
        WebSocket WSock;
        EventHandler<MessageEventArgs> OnData;
        public string Streamer,ConnectMessage;
        public Twitch(string Stream, EventHandler<MessageEventArgs> onData, string connectMessage = "")
        {
            OnData = onData;
            Streamer = Stream;
            ConnectMessage = connectMessage;
            WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
            WSock.OnClose += WSock_Closed;
            WSock.OnMessage += WSock_MessageReceived;
            WSock.OnOpen += WSock_Opened;
            WSock.Connect();
        }

        public void SendMessage(string text)
        {
#if DEBUG
            WSock.Send("PRIVMSG #" + Streamer + " :[DEBUG] " + text);
#else
            WSock.Send("PRIVMSG #" + Streamer + " :" + text);
#endif
        }

        public void SetTimeout(string userLogin, string chanelID,string expiresIn)
        {
            //[{"operationName":"Chat_BanUserFromChatRoom","variables":{"input":{"channelID":"58206658","bannedUserLogin":"sex","expiresIn":"10s","reason":null}},"extensions":{"persistedQuery":{"version":1,"sha256Hash":"b3a86ecf228824820543f7190362650e727b66d980b34722e27272d461410514"}}}]
            SendModerator("\"operationName\":\"Chat_BanUserFromChatRoom\",\"variables\":{\"input\":{\"channelID\":\"" + chanelID + "\",\"bannedUserLogin\":\""+ userLogin + "\",\"expiresIn\":\""+ expiresIn + "s\"}}",
                TwitchAccount.Current.timeoutSHA);
        }
        public void DeleteMessage(string messageID, string chanelID)
        {
            //send post to https://gql.twitch.tv/gql
            //[{"operationName":"Chat_DeleteChatMessage","variables":{"input":{"channelID":"58206658","messageID":"48a0b454-3f48-4b18-950e-38b4697e244a"}},"extensions":{"persistedQuery":{"version":1,"sha256Hash":"b3a86ecf228824820543f7190362650e727b66d980b34722e27272d461410514"}}}]
            SendModerator("\"operationName\":\"Chat_DeleteChatMessage\",\"variables\":{\"input\":{\"channelID\":\"" + chanelID + "\",\"messageID\":\"" + messageID + "\"}}",
                TwitchAccount.Current.deleteSHA);
        }
        private string SendModerator(string Data,string Hash)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://gql.twitch.tv/gql");

            var postData = "[{"+Data+",\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\""+ Hash + "\"}}}]";
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "text/plain;charset=UTF-8";
            request.ContentLength = data.Length;

            request.Headers.Add("Authorization", "OAuth "+ TwitchAccount.Current.moderToken);
            //request.Headers.Add("Client-Id", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            request.Referer = @"https://www.twitch.tv/" + Streamer;
            //request.Headers.Add("Referer", @"https://www.twitch.tv/"+Streamer);

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }

        private void WSock_Opened(object sender, EventArgs e)
        {
            WSock.Send("CAP REQ :twitch.tv/tags twitch.tv/commands");

            WSock.Send("PASS oauth:" + TwitchAccount.Current.token);
            WSock.Send("NICK "+ TwitchAccount.Current.login);
            WSock.Send("USER "+ TwitchAccount.Current.login + " 8 * :"+ TwitchAccount.Current.login);
        }

        private void WSock_MessageReceived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (e.Data.Contains(":tmi.twitch.tv CAP * ACK :twitch.tv/tags twitch.tv/commands"))
            {
                WSock.Send("JOIN #" + Streamer);
                if (!string.IsNullOrEmpty(ConnectMessage))
                {
                    Thread.Sleep(200);
                    SendMessage(ConnectMessage);
                }
            }
            else if(e.Data.Contains("PING :tmi.twitch.tv"))
            {
                WSock.Send("PONG");
            }
            else
            {
                if (e.Data.Contains("PRIVMSG")) {
                    string nick = "", id = "", chanel = "";
                    string[] splited = e.Data.Split(';');
                    ExMsgFlag msgflag = ExMsgFlag.None;
                    foreach (var x in splited)
                    {
                        string[] datargs = x.Split('=');
                        if (datargs.Length == 2)
                        {
                            switch (datargs[0])
                            {
                                case "display-name":
                                    nick = datargs[1];
                                    break;
                                case "id":
                                    id = datargs[1];
                                    break; 
                                case "room-id":
                                    chanel = datargs[1];
                                    break;
                                //msg-id=highlighted-message
                                case "msg-id":
                                    switch (datargs[1])
                                    {
                                        case "highlighted-message":
                                            msgflag |= ExMsgFlag.Highlighted;
                                            break;
                                    }
                                    break;
                                case "mod":
                                    if (datargs[1] == "1")
                                        msgflag |= ExMsgFlag.FromModer;
                                    break;
                                case "subscriber":
                                    if (datargs[1] == "1")
                                        msgflag |= ExMsgFlag.FromSub;
                                    break;
                            }
                        }

                    }
                    OnData.Invoke(this,
                        new MessageEventArgs(nick, e.Data.Split(new string[] { "PRIVMSG #" + Streamer + " :" },StringSplitOptions.RemoveEmptyEntries).Last(), id, chanel, msgflag));
                }
            }
        }

        private void WSock_Closed(object sender, EventArgs e)
        {
            WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
            WSock.OnClose += WSock_Closed;
            WSock.OnMessage += WSock_MessageReceived;
            WSock.OnOpen += WSock_Opened;
            WSock.Connect();
        }
    }
    public class MessageEventArgs
    {
        public string NickName, Message, ID, Chanel;
        public ExMsgFlag Flags;
        public MessageEventArgs(string nickName, string message, string id, string chanel, ExMsgFlag flags = ExMsgFlag.None)
        {
            NickName = nickName;
            Message = message;
            ID = id;
            Chanel = chanel;
            Flags = flags;
        }
    }
    [Flags]
    public enum ExMsgFlag
    {
        None = 0,
        Highlighted = 1,
        FromModer = 2,
        FromSub = 4
    }
}
