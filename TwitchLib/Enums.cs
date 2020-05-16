using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLib
{
    [Flags]
    public enum ExMsgFlag
    {
        None = 0,
        FromModer = 1,
        FromSub = 2,
        FromVip = 4,
        HasPrime = 8,
        HasTurbo = 16,
        HasGLHF = 32,
        Highlighted = 64,
        SubModeSkiped = 128
    }
    public enum BanType
    {
        BanOrTimeout,
        MsgDelete
    }
}
