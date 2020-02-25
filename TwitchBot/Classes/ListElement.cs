using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchBot
{
    class ListElement
    {
        public int ID { get; set; }
        public string[] Strings { get; set; }
        public int[] Nums { get; set; }
        public ListElement(int id,int stringCount,int numCount)
        {
            ID = id;
            Strings = new string[stringCount];
            Nums = new int[numCount];
        }
        public ListElement Duplicate()
        {
            ListElement New = new ListElement(ID+1,Strings.Length,Nums.Length);;
            New.Nums = (int[])Nums.Clone();
            New.Strings = (string[])Strings.Clone();
            return New;
        }
    }
}
