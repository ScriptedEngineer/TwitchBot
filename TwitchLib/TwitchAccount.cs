using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TwitchLib
{
    public class TwitchAccount
    {
        public string Login, Token, ClientID, UserID, Scopes;
        public TwitchAccount(string login,string token)
        {
            Login = login;
            Token = token;
            new Task(() =>
            {
                Validate();
            }).Start();
        }
        public TwitchAccount()
        {
            Login = "justinfan99999";
            Token = "SCHMOOPIIE";
        }
        public bool Validate()
        {
            WebRequest reqGetUser = WebRequest.Create("https://id.twitch.tv/oauth2/validate");
            reqGetUser.Headers["Authorization"] = $"OAuth {Token}";
            string content = Web.GetResponse(reqGetUser.GetResponse());
            Match User = Regex.Match(content, @"""client_id"":""(\w*)""\S*""login"":""(\w*)"".*""scopes"":\[([^\]]*)\],""user_id"":""(\w*)""");
            ClientID = User.Groups[1].Value;
            Scopes = User.Groups[3].Value; 
            UserID = User.Groups[4].Value;
            return Login == User.Groups[2].Value;
        }
        public static string GetLogin(string Token)
        {
            WebRequest reqGetUser = WebRequest.Create("https://id.twitch.tv/oauth2/validate");
            reqGetUser.Headers["Authorization"] = $"OAuth {Token}";
            string content = Web.GetResponse(reqGetUser.GetResponse());
            Match User = Regex.Match(content, @"""client_id"":""(\w*)""\S*""login"":""(\w*)"".*""user_id"":""(\w*)""");
            return User.Groups[2].Value;
        }

        public bool CheckScopes(params string[] scopes)
        {
            if (!string.IsNullOrEmpty(Scopes))
            {
                bool inside = true;
                foreach(string scope in scopes)
                {
                    inside &= Scopes.Contains(scope);
                }
                return inside;
            }
            return false;
        }
    }
}
