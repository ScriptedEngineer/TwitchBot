using System;
using System.IO;
using System.Net;
using WebSocketSharp;
using System.Text;
using System.Text.RegularExpressions;

namespace TwitchBot.Classes
{
    public class DonationAlerts
    {
        string Token,Refresh_token;
        public EventHandler<DonationEventArgs> OnDonation;
        WebSocket WSock;
        string UserID, CentClID;
        string SockTocken;
        public bool Connected { get; private set; }
        public DonationAlerts(string token, string refresh_token)
        {
            Connected = false;
            Token = RefreshToken(refresh_token, token);
            Refresh_token = refresh_token;
            GetConnectToken();
            WSock = new WebSocket("wss://centrifugo.donationalerts.com/connection/websocket");
            WSock.OnClose += WSock_OnClose; 
            WSock.OnMessage += WSock_OnMessage;
            WSock.OnOpen += WSock_OnOpen;
            WSock.Connect();
        }

        private void WSock_OnOpen(object sender, EventArgs e)
        {

            WSock.Send("{\"params\":{\"token\":\""+ SockTocken+"\"},\"id\":1}");
        }

        private void WSock_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            //Console.WriteLine(e.Data);
            try {
                Match IDM = Regex.Match(e.Data, @"""type"":(\d*)");
                string switcher = IDM.Groups[1].Value;
                if (!IDM.Success)
                {
                    IDM = Regex.Match(e.Data, @"""id"":(\d*)");
                    switcher = "id"+IDM.Groups[1].Value;
                }
                switch (switcher)
                {
                    case "id1":
                        Match CLID = Regex.Match(e.Data, @"""client"":""([^""]*)""");
                        CentClID = CLID.Groups[1].Value;
                        string ctoken = SubscribeOnPChan();
                        WSock.Send("{\"params\":{\"channel\":\"$alerts:donation_" + UserID + "\",\"token\":\"" + ctoken + "\"},\"method\":1,\"id\":2}");
                        break;
                    case "1":
                        //Console.WriteLine(e.Data);
                        Connected = true;
                        break;
                    default:
                        Match CLsID = Regex.Match(e.Data, @"""name"":""([^""]*)""");
                        if(CLsID.Groups[1].Value == "donation")
                        {
                            string lal = Regex.Unescape(e.Data);//WebUtility.HtmlDecode(
                            Match Nick = Regex.Match(lal, @"""username"":""([^""]*)""");
                            Match Message = Regex.Match(lal, @"""message"":""([^""]*)""");
                            Match Amount = Regex.Match(lal, @"""amount"":(\d*)");
                            Match MessageType = Regex.Match(lal, @"""message_type"":""([^""]*)""");
                            Match Currency = Regex.Match(lal, @"""currency"":""([^""]*)""");
                            OnDonation.Invoke(this, new DonationEventArgs(Nick.Groups[1].Value, Message.Groups[1].Value,Amount.Groups[1].Value,MessageType.Groups[1].Value,Currency.Groups[1].Value));
                        }
                        //System.Net.WebUtility.HtmlDecode(test);

                        break;
                }
            } catch {
            }
        }

        private void WSock_OnClose(object sender, CloseEventArgs e)
        {
            
        }

        public static string RefreshToken(string refresh_token, string token)
        {
            //grant_type=refresh_token&refresh_token=<refresh_token>&client_id=<client_id>&client_secret=<client_secret>&scope=<scope>
            string post = $"grant_type=refresh_token&refresh_token={refresh_token}&client_id=865&client_secret=TVKEuWoskmVWYDrQ8Qy8mJPliVG3qYXzmFmaan3l&scope=oauth-donation-subscribe+oauth-user-show";
            HttpWebRequest CodeReq = (HttpWebRequest)WebRequest.Create("https://www.donationalerts.com/oauth/token");
            //reqFollow.Timeout = 10;
            CodeReq.Method = "POST";
            CodeReq.ContentType = "application/x-www-form-urlencoded";
            byte[] byteArray = Encoding.UTF8.GetBytes(post);
            CodeReq.ContentLength = byteArray.Length;
            Stream dataStream = CodeReq.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                WebResponse response = CodeReq.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string SRTEND = readStream.ReadToEnd();
                //Console.WriteLine(SRTEND);
                Match Rgex = Regex.Match(SRTEND, @"""access_token"":""([^""]*)"".*""refresh_token"":""([^""]*)""");
                try
                {
                    try
                    {
                        File.WriteAllText("da.txt", Rgex.Groups[1].Value + "\n" + Rgex.Groups[2].Value);
                    }
                    catch { }
                    return Rgex.Groups[1].Value;
                }
                catch
                {
                    return token;
                }
            }
            catch (WebException e)
            {
                WebResponse response = e.Response;
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                File.Delete("da.txt");
                Console.WriteLine(readStream.ReadToEnd());
                return token;
            }
        }
        public void GetConnectToken()
        {
            HttpWebRequest CodeReq = (HttpWebRequest)WebRequest.Create("https://www.donationalerts.com/api/v1/user/oauth");
            CodeReq.Method = "GET";
            CodeReq.ContentType = "application/x-www-form-urlencoded";
            CodeReq.Headers["Authorization"] = $"Bearer {Token}";
            try
            {
                WebResponse response = CodeReq.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string SRTEND = readStream.ReadToEnd();
                //Console.WriteLine(SRTEND);
                Match Rgex = Regex.Match(SRTEND, @"""socket_connection_token"":""([^""]*)""");
                Match Rgex2 = Regex.Match(SRTEND, @"""id"":(\d*)");
                try
                {
                    SockTocken = Rgex.Groups[1].Value;
                    UserID = Rgex2.Groups[1].Value;
                }
                catch
                {
                }
            }
            catch (WebException e)
            {
                WebResponse response = e.Response;
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                //File.Delete("da.txt");
                Console.WriteLine(readStream.ReadToEnd());
            }
        }
        public string SubscribeOnPChan()
        {
            string post = "{\"channels\":[\"$alerts:donation_" + UserID+ "\"], \"client\":\"" + CentClID + "\"}";
            HttpWebRequest CodeReq = (HttpWebRequest)WebRequest.Create("https://www.donationalerts.com/api/v1/centrifuge/subscribe");
            CodeReq.Method = "POST";
            CodeReq.ContentType = "application/json";
            CodeReq.Headers["Authorization"] = $"Bearer {Token}";
            byte[] byteArray = Encoding.UTF8.GetBytes(post);
            CodeReq.ContentLength = byteArray.Length;
            Stream dataStream = CodeReq.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            try
            {
                WebResponse response = CodeReq.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string SRTEND = readStream.ReadToEnd();
                //Console.WriteLine(SRTEND);
                Match Rgex2 =  Regex.Match(SRTEND, @"""token"":""([^""]*)""");
                try
                {
                    return Rgex2.Groups[1].Value;
                }
                catch
                {
                    return null;
                }
            }
            catch (WebException e)
            {
                WebResponse response = e.Response;
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                //File.Delete("da.txt");
                Console.WriteLine(readStream.ReadToEnd());
                return null;
            }
        }
    }
    public class DonationEventArgs
    {
        public string NickName, Message, Amount, MessageType, Currency;
        public DonationEventArgs(string nickName, string message, string amount, string type, string currency)
        {
            NickName = nickName;
            Message = message;
            Amount = amount;
            MessageType = type;
            Currency = currency;
        }
    }
}
