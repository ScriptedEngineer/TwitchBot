using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLib
{
    public class Web
    {
        public static string GetResponse(WebResponse wr)
        {
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.ASCII);
            return reader.ReadToEnd();
        }
    }
}
