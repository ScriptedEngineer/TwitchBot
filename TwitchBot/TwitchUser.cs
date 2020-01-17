using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class TwitchUser
    {
        public DateTime LastMessageTime;
        public int SpamCount,Timeouts;
        public string LastMessage;
        public TwitchUser(string Message = "")
        {
            SpamCount = 0;
            Timeouts = 15;
            LastMessageTime = DateTime.Now;
            LastMessage = Message;
        }
    }
}
