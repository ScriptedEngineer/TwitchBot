using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    public class Voter
    {
        public string Nickname { get; }
        public string Vote { get; set; }
        public Voter(string nickname , string vote)
        {
            Nickname = nickname;
            Vote = vote;
        }
    }
}
