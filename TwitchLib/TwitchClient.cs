using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WebSocketSharp;
using Timer = System.Timers.Timer;

namespace TwitchLib
{
    public class TwitchClient
    {
        WebSocket WSock, WSock1;
        public EventHandler<MessageEventArgs> OnMessage;
        public EventHandler<RewardEventArgs> OnReward;
        public EventHandler<BanEventArgs> OnBan;
        public string Streamer, ConnectMessage, About;
        public string StreamerID = null;
        public TwitchAccount Account;
        bool Listener;
        string SendedMessage;
        public TwitchClient(TwitchAccount account, string Stream, string connectMessage = "", string about = "", bool listener = false, bool pubsub = false)
        {
            Account = account;
            About = about;
            Streamer = Stream;
            Listener = listener;
            ConnectMessage = connectMessage;
            WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
            WSock.OnClose += WSock_Closed;
            WSock.OnMessage += WSock_MessageReceived;
            WSock.OnOpen += WSock_Opened;
            if (listener || pubsub)
                new Task(() =>
                {
                    WSock1 = new WebSocket("wss://pubsub-edge.twitch.tv/v1");
                    WSock1.OnClose += WSock1_Closed;
                    WSock1.OnMessage += WSock1_MessageReceived;
                    WSock1.OnOpen += WSock1_Opened;
                }).Start();
        }

        public void SetHttpProxy(string addr, string user = "", string pass = "")
        {
            WSock.SetProxy(addr, user, pass);
        }

        public bool IsFollow(string StreamerID)
        {
            try
            {
                HttpWebRequest reqFollow = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.UserID}/follows/channels/{StreamerID}");
                reqFollow.Method = "GET";
                reqFollow.Accept = "application/vnd.twitchtv.v5+json";
                reqFollow.Headers["Client-ID"] = Account.ClientID;
                reqFollow.Headers["Authorization"] = $"OAuth {Account.Token}";
                string content = Web.GetResponse(reqFollow.GetResponse());
                if (Regex.IsMatch(content, @"created_at"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public void Follow(string StreamerID)
        {
            new Task(() =>
            {
                if (IsFollow(StreamerID))
                return;
                HttpWebRequest reqFollow = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.UserID}/follows/channels/{StreamerID}");
                //reqFollow.Timeout = 10;
                reqFollow.Method = "PUT";
                reqFollow.Accept = "application/vnd.twitchtv.v5+json";
                reqFollow.ContentType = "application/json";
                reqFollow.Headers["Client-ID"] = Account.ClientID;
                reqFollow.Headers["Authorization"] = $"OAuth {Account.Token}";
                try
                {
                    WebResponse response = reqFollow.GetResponse();
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    Console.WriteLine(readStream.ReadToEnd());
                }
                catch(WebException e)
                {
                    WebResponse response = e.Response;
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    Console.WriteLine(readStream.ReadToEnd());
                }
            }).Start();
            //string content3 = Web.GetResponse();
            //Console.WriteLine(content3);
        }
        public void Unfollow(string StreamerID)
        {
            new Task(() =>
            {
                if (!IsFollow(StreamerID))
                return;
                HttpWebRequest reqFollow = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/kraken/users/{Account.UserID}/follows/channels/{StreamerID}");
                reqFollow.Timeout = 10;
                reqFollow.Method = "DELETE";
                reqFollow.Accept = "application/vnd.twitchtv.v5+json";
                reqFollow.Headers["Client-ID"] = Account.ClientID;
                reqFollow.Headers["Authorization"] = $"OAuth {Account.Token}";
                try
                {
                    reqFollow.GetResponse();
                }
                catch
                {

                }
            }).Start();
        }

        public string GetStreamerID()
        {
            while (Account.ClientID == null)
                Thread.Sleep(500);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            HttpWebRequest reqGetChannel = (HttpWebRequest)WebRequest.Create($"https://api.twitch.tv/helix/users?login={Streamer}");
            reqGetChannel.Headers["Authorization"] = $"Bearer {Account.Token}";
            reqGetChannel.Headers["Client-ID"] = $"{Account.ClientID}";
            WebResponse response;
            try
            {
                response = reqGetChannel.GetResponse();
                string content2 = Web.GetResponse(response);
                Match Channel = Regex.Match(content2, @"""id"":""(\d*)""");
                StreamerID = Channel.Groups[1].Value;
                return StreamerID;
            }
            catch (WebException ex)
            {
                response = (ex.Response as WebResponse);
                if (response != null)
                    Console.WriteLine(Web.GetResponse(response));
                return "0";
            }
        }

        public void Host()
        {
            new Task(() =>
            {
                TwitchClient x = new TwitchClient(Account, Account.Login, $"/host {Streamer}", "", false);
                x.Connect();
                Thread.Sleep(6000);
                x.Close();
            }).Start();
        }
        public void Unhost()
        {
            new Task(() =>
            {
                TwitchClient x = new TwitchClient(Account, Account.Login, $"/unhost", "", false);
                x.Connect();
                Thread.Sleep(6000);
                x.Close();
            }).Start();
        }


        public void ConnectToOther(string streamer)
        {
            WSock.Send("PART #" + Streamer);
            Streamer = streamer;
            WSock.Send("JOIN #" + Streamer);
        }
        public void Part()
        {
            WSock.Send("PART #" + Streamer);
        }

        bool Closed = false;
        public void Connect()
        {
            Closed = false;
            StreamerID = GetStreamerID();
            WSock.Connect();
            WSock1?.Connect();
        }
        public void Disconnect()
        {
            Closed = true;
            WSock.Close();
            WSock1?.Close();
        }

        public void SendMessage(string text)
        {
            //if (string.IsNullOrEmpty(StreamerID))
              //  Connect();
            if (!WSock.IsAlive)
            {
                WSock.Close();
                //WSock_Closed(null, null);
            }
            try
            {
                WSock.Send("PRIVMSG #" + Streamer + " :" + text);
                SendedMessage = text;
            }
            catch
            {

            }
        }

        private void WSock_Opened(object sender, EventArgs e)
        {
            WSock.Send("CAP REQ :twitch.tv/tags twitch.tv/commands");

            WSock.Send("PASS oauth:" + Account.Token);
            WSock.Send("NICK " + Account.Login);
            WSock.Send("USER " + Account.Login + " 8 * :" + Account.Login);
        }
        private void WSock_MessageReceived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            //Console.WriteLine(e.Data);
            if (e.Data.Contains(":tmi.twitch.tv CAP * ACK :twitch.tv/tags twitch.tv/commands"))
            {
                WSock.Send("JOIN #" + Streamer);
                new Task(() => { 
                if (!string.IsNullOrEmpty(ConnectMessage))
                {
                    Thread.Sleep(200);
                    SendMessage(ConnectMessage);
                }
                }).Start();
            }
            else if (e.Data.Contains("PING :tmi.twitch.tv"))
            {
                WSock.Send("PONG");
            }
            else if(Listener)
            {
                bool davulf = false;
                if (e.Data.Contains("PRIVMSG") || ((davulf = e.Data.Contains("USERSTATE")) && !string.IsNullOrWhiteSpace(SendedMessage)))
                {
                    string data = e.Data.Trim();
                    if (davulf)
                    {
                        data += " :" + SendedMessage;
                        SendedMessage = "";
                    }
                    string nick = "", id = "", userid = "", chanel = "", crid = "";
                    string[] splited = data.Split(';');
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
                                case "user-id":
                                    userid = datargs[1];
                                    break;
                                case "room-id":
                                    chanel = datargs[1];
                                    break;
                                case "badges":
                                    foreach (var bad in datargs[1].Split(','))
                                    {
                                        switch (bad.Split('/')[0])
                                        {
                                            case "vip":
                                                msgflag |= ExMsgFlag.FromVip;
                                                break;
                                            case "glhf-pledge":
                                                msgflag |= ExMsgFlag.HasGLHF;
                                                break;
                                            case "subscriber":
                                                break;
                                            case "premium":
                                                msgflag |= ExMsgFlag.HasPrime;
                                                break;
                                        }
                                    }
                                    break;
                                case "msg-id":
                                    switch (datargs[1])
                                    {
                                        case "highlighted-message":
                                            msgflag |= ExMsgFlag.Highlighted;
                                            break;
                                        case "skip-subs-mode-message":
                                            msgflag |= ExMsgFlag.SubModeSkiped;
                                            break;
                                    }
                                    break;
                                case "custom-reward-id":
                                    crid = datargs[1];
                                    break;
                                case "turbo":
                                    if (datargs[1] == "1")
                                        msgflag |= ExMsgFlag.HasTurbo;
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
                    OnMessage.Invoke(this,
                        new MessageEventArgs(nick, data.Split(new string[] { "#" + Streamer + " :" }, StringSplitOptions.RemoveEmptyEntries).Last(), userid, id, chanel, msgflag, crid));
                }
                else if (e.Data.Contains("CLEARCHAT"))
                {
                    int duration = 0;
                    string[] splited = e.Data.Split(';');
                    foreach (var x in splited)
                    {
                        string[] datargs = x.Split('=');
                        if (datargs.Length == 2)
                        {
                            switch (datargs[0])
                            {
                                case "@ban-duration":
                                    int.TryParse(datargs[1],out duration);
                                    break;
                            }
                        }
                    }
                    string userName = e.Data.Split(new string[] { "CLEARCHAT #" + Streamer + " :" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    OnBan.Invoke(this, new BanEventArgs(userName.Trim(), duration, BanType.BanOrTimeout));
                }
                else if (e.Data.Contains("CLEARMSG"))
                {
                    string userName = "", MsgID = "";
                    string[] splited = e.Data.Split(';');
                    foreach (var x in splited)
                    {
                        string[] datargs = x.Split('=');
                        if (datargs.Length == 2)
                        {
                            switch (datargs[0])
                            {
                                case "@login":
                                    userName = datargs[1];
                                    break;
                                case "target-msg-id":
                                    MsgID = datargs[1];
                                    break;
                            }
                        }
                    }
                    string Msg = e.Data.Split(new string[] { "CLEARMSG #" + Streamer + " :" }, StringSplitOptions.RemoveEmptyEntries).Last();
                    OnBan.Invoke(this, new BanEventArgs(userName.Trim(), Msg, MsgID, BanType.MsgDelete));
                }
            }
        }
        private void WSock_Closed(object sender, EventArgs e)
        {
            if (Closed)
                return;
            WSock = new WebSocket("wss://irc-ws.chat.twitch.tv/");
            WSock.OnClose += WSock_Closed;
            WSock.OnMessage += WSock_MessageReceived;
            WSock.OnOpen += WSock_Opened;
            WSock.Connect();
        }

        private void WSock1_Opened(object sender, EventArgs e)
        {
            WSock1.Send($"{{\"type\":\"LISTEN\",\"nonce\":\"{RandomString(30)}\",\"data\":{{\"topics\":[\"community-points-channel-v1.{StreamerID}\"]}}}}");
            //WSock1.Send($"{{\"type\":\"LISTEN\",\"nonce\":\"{RandomString(30)}\",\"data\":{{\"topics\":[\"channel-points-channel-v1.{StreamerID}\"],\"auth_token\":\"{Account.Token}\"}}}}");
            //WSock1.Send($"{{\"type\":\"LISTEN\",\"nonce\":\"{RandomString(30)}\",\"data\":{{\"topics\":[\"chat_moderator_actions.{StreamerID}\"],\"auth_token\":\"{Account.Token}\"}}}}");
            //WSock1_MessageReceived(null,null);
        }
        private void WSock1_MessageReceived(object sender, WebSocketSharp.MessageEventArgs e)
        {
            if (Listener)
                try
                {
                    string data = e.Data;
                    if (data.Contains("\"type\":\"MESSAGE\""))
                    {
                        if (data.Contains("\\\"type\\\":\\\"reward-redeemed\\\""))
                        {
                            string nbs = data.Replace("\\", "");
                            Match rewrad = Regex.Match(nbs, @"""reward""\W*""id"":""([\w-]*)""");
                            Match user = Regex.Match(nbs, @"""user""\W*""id"":""(\d*)""");
                            Match redem = Regex.Match(nbs, @"""redemption""\W*""id"":""([\w-]*)""");
                            Match login = Regex.Match(nbs, @"""display_name"":""(\w*)""");
                            Match title = Regex.Match(nbs, @"""title"":""([^""]*)""");
                            Match chanel = Regex.Match(nbs, @"""channel_id"":""(\d*)""");
                            Match text = Regex.Match(nbs, @"""user_input"":""([^""]*)""");
                            new Thread(() =>
                            {
                                OnReward.Invoke(this, new RewardEventArgs(login.Groups[1].ToString(), rewrad.Groups[1].ToString(), user.Groups[1].ToString(), redem.Groups[1].ToString(), chanel.Groups[1].ToString(), title.Groups[1].ToString(), text.Groups[1].ToString()));
                            }).Start();
                        }
                        //Console.WriteLine(e.Data);
                    }
                }
                catch (NullReferenceException ex)
                {

                }
        }
        private void WSock1_Closed(object sender, EventArgs e)
        {
            if (Closed)
                return;
            WSock1 = new WebSocket("wss://pubsub-edge.twitch.tv/v1");
            WSock1.OnClose += WSock1_Closed;
            WSock1.OnMessage += WSock1_MessageReceived;
            WSock1.OnOpen += WSock1_Opened;
            WSock1.Connect();
        }
        private IDictionary<string, object> OP(object x)=> (IDictionary<string, object>)x;
        
        public void Close()
        {
            Part();
            WSock.Close();
            WSock1?.Close();
        }

        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
