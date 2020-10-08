using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class MyEncription
    {
        public static void SaveCryptoFile(string path, params string[] lines)
        {
            /*This code hided for your safety!*/
            /*Этот код скрыт для вашей безопасности!*/
            File.WriteAllLines(path, lines);
        }
        public static string[] ReadCryptoFile(string path)
        {
            /*This code hided for your safety!*/
            /*Этот код скрыт для вашей безопасности!*/
            return File.ReadAllLines(path);
        }
    }
}
