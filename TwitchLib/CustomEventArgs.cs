using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLib
{
    public class MessageEventArgs
    {
        public string NickName, Message, ID, Chanel, UserID, CustomRewardID;
        public ExMsgFlag Flags;
        public MessageEventArgs(string nickName, string message, string userid, string id, string chanel, ExMsgFlag flags = ExMsgFlag.None, string crid = "")
        {
            NickName = nickName;
            Message = message;
            ID = id;
            Chanel = chanel;
            Flags = flags;
            UserID = userid;
            CustomRewardID = crid;
        }
    }
    public class RewardEventArgs
    {
        public string NickName, ID, Chanel, UserID, CustomRewardID, Title, Text;
        public RewardEventArgs(string nickName, string crid, string userid, string id, string chanel, string title, string text)
        {
            NickName = nickName;
            ID = id;
            Chanel = chanel;
            UserID = userid;
            CustomRewardID = crid;
            Title = title;
            Text = text;
        }
    }
    public class BanEventArgs
    {
        public string NickName, DeletedMessage, MessageID;
        public int Duration;
        public BanType Type;
        public BanEventArgs(string nickName, int druation, BanType type = BanType.BanOrTimeout)
        {
            NickName = nickName;
            Duration = druation;
            Type = type;
            DeletedMessage = null;
            MessageID = null;
        }
        public BanEventArgs(string nickName, string deletedMessage, string MsgID, BanType type = BanType.MsgDelete)
        {
            NickName = nickName;
            Duration = 0;
            Type = type;
            DeletedMessage = deletedMessage;
            MessageID = MsgID;
        }
    }
}
