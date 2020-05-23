using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib;

namespace TwitchBot
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public static string WebRequestу(HttpListenerRequest request)
        {
            switch (request.RawUrl.Split('?').First().Trim('/'))
            {
                case "obs":
                    return Properties.Resources.ServerMain;
                case "control":
                    return "NO";
                case "twitchcode":
                    return "<script>location.replace('http://localhost:8190/twithctoken?' + window.location.href.split('#')[1]);</script>";
                case "twithctoken":
                    string token = request.QueryString.Get("access_token");
                    string login = TwitchAccount.GetLogin(token);
                    File.WriteAllLines("account.txt", new string[] { login, token });
                    return "<script>location.replace('https://wsxz.ru/closetab');</script>";
                case "da":
                    new Thread(() =>
                    {
                        string code = request.QueryString.Get("code");
                        string post = $"grant_type=authorization_code&client_id=865&client_secret=TVKEuWoskmVWYDrQ8Qy8mJPliVG3qYXzmFmaan3l&redirect_uri=http://localhost:8190/da&code={code}";
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
                            //Console.WriteLine();//{"token_type":"Bearer","expires_in":631152000,"access_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImp0aSI6IjYxYTc1Yjk1NWY2YzkyZGZiOGYwMmZjNzc0MzE0OWYwOGUwYzY0YTQ5MjJiOTZkM2M5YTUzZDQ4YTk5MTFmNTIwYmU2YmYzM2JiNzY2NzJmIn0.eyJhdWQiOiI4NjUiLCJqdGkiOiI2MWE3NWI5NTVmNmM5MmRmYjhmMDJmYzc3NDMxNDlmMDhlMGM2NGE0OTIyYjk2ZDNjOWE1M2Q0OGE5OTExZjUyMGJlNmJmMzNiYjc2NjcyZiIsImlhdCI6MTU5MDE2OTEwNCwibmJmIjoxNTkwMTY5MTA0LCJleHAiOjIyMjEzMjExMDQsInN1YiI6IjM0NDg3NzQiLCJzY29wZXMiOlsib2F1dGgtZG9uYXRpb24tc3Vic2NyaWJlIl19.NTKPCsiR09sI-HOUvkfsY9Q2mHpJWizCUI_oTA-5VzBYB0YIaYfwVcoNLt5eAQMjoNBLL4o86HOFkHGQokQ5x9NKNeJjDStB1UmwylD-Xmba8Zsyv-QTOTUTi8WWi0ABMBSVY_MbBrwOTm_4_n-XlDSakm7yzL0tJijZLgEPfnd3_983VgZQ4IZA0GK57tLJyGh7r544R9eMARGiksCdQKU7kP0DyQQNbckQsRthyoYZCGs6cNpZJRTav5jeV9obQ2J0vJzL2RuxrRDKc0-MWc6gj8jLJVO3dG9X-rYGSp6ALJz61e7ZShkcsuHA2q0OdFUmiW7fTQnD9zLw2_nd9yA15Bu2qsu5f8VBMtLUlSUzykvi-iWDRrmw8oEgwGUjY8CARJ28xAFS3rfyvk8MuDeVW4Q07leiC9ttOjEp59SgpXMpJN_Zo2P5Q7PrP868G1LSK9WabNbnKvVXVIgNIOYFrGTj7NbWk-JOkHEz6tc5WhT9WF3Yf46rMWww2-POrdbqbpMA7YTd440n2nSrNzco-vHZkQg1jolW0lG4EUzjX5uR2v6M98D2X-VvgNdGs1m6E0cz0v0bVo7xKlKij06ZX25zC3O2tj99oMWsW0mzZBDdbrLjeLYXTJZTDa9jdjbbcn2y8-0cXUOOu6D9_djBTHd-LrE40gkMK_B4ohQ","refresh_token":"def50200dca80dfde5c2603d82075af41efb9240212b7732bcf6eeb613dca0f266608e74f2f1de5926cf9f3047681a3fd08173748566f3f38a625cf92fec7e9a321185561e2c923e83a0fb96a6b1583271ca57f7609d319f731572ec055e7b297069a7cc8cb2b893b068378a5295ceb96e7fad440adfb13a2aa37f4bac344f80bfd2e424442dfc61a3d01dd67e047ea2517f8bd7b426070b568c9267f7bd1cd20e75ee4b70a9ba32bc497cc2ae23d65126e256e326f7081367a74144fabbac7df3c4fbab8a5e20592cc880190c35ac412defe413d431a84bd13037f70ea53be486233e3047aa2dc77582aa6278b822363ed2d5c0566dc372afaf26e90cfdff404cebe850347d8475dabe951682e734b3854491a50ce915f88cbc15a19a27392bca8cb4938d6f1427c20accdde140ad3bcfda1970e54202aaf980c3d82a3d15fb29799e1a429a01c96cb948ccf419be4925223c4e497c7314cb24e0ddd70a88be566246e60a4de20370d52c244f14b90d987f593abade5c948953f2dd4f63ebfcb1ed4b"}
                            Match Rgex = Regex.Match(readStream.ReadToEnd(), @"""access_token"":""([^""]*)"".*""refresh_token"":""([^""]*)""");
                            try
                            {
                                File.WriteAllText("da.txt", Rgex.Groups[1].Value +"\n"+ Rgex.Groups[2].Value);
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
                            Console.WriteLine(readStream.ReadToEnd());
                        }
                        //
                    }).Start();
                    return "<script>location.replace('https://wsxz.ru/closetab');</script>";
                default:
                    return "Not found";
            }
        }

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("responder method required");
            }

            foreach (var s in prefixes)
            {
                _listener.Prefixes.Add(s);
            }

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(params string[] prefixes)
           : this(prefixes, WebRequestу)
        {
        }
        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
           : this(prefixes, method)
        {
        }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                //Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var rstr = _responderMethod(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                // always close the stream
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
                                }
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
