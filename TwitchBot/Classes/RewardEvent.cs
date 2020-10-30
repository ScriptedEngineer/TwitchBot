using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TwitchBot.Classes;
using TwitchLib;

namespace TwitchBot
{
    public class RewardEvent
    {
        public string CustomRewardID,
            RewardName = "Определено пользователем",
            EventName = "Новый",
            Script;
        public bool DeferredRun = false;
        private bool runEveryTime = false;
        private bool Runing;
        private readonly List<string> Deferred = new List<string>();
        private string DefereData = "";
        public RewardEvent()
        {

        }
        public void Invoke(RewardEventArgs e)
        {
            while (Runing)
                Thread.Sleep(100);
            Runing = true;
            string scripd = Script;
            if (scripd != null && scripd.Contains("%"))
            {

                if (scripd.Contains("%TEXT%"))
                    scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", "").Trim());
                if (scripd.Contains("%NICK%"))
                    scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", "").Trim());
                if (scripd.Contains("%TITLE%"))
                    scripd = scripd.Replace("%TITLE%", e.Title.Replace("\n", "").Trim());
            }
            if (DeferredRun && !runEveryTime)
            {
                Deferred.Add(scripd);
                Runing = false;
            }
            else
            {
                if (scripd.Contains("%DATA%"))
                    scripd = scripd.Replace("%DATA%", DefereData);
                ScriptLanguage.RunScript(scripd);
                if(DeferredRun) Thread.Sleep(1000);
                Runing = false;
            }
        }
        public void InvokeDeferred(string data)
        {
            DefereData = data;
            new Task(() => {
                while (Runing)
                    Thread.Sleep(100);
                Runing = true;
                foreach (string scrip in Deferred) {
                    string scripd = scrip;
                    if (scripd.Contains("%DATA%"))
                        scripd = scripd.Replace("%DATA%", data);
                    ScriptLanguage.RunScript(scripd);
                    Thread.Sleep(1000);
                }
                Deferred.Clear();
                Runing = false;
            }).Start();
        }
        public void SetET(bool ET) => runEveryTime = ET;
        public bool GetET() => runEveryTime;
    }
    public class ComandEvent
    {
        public string Comand = "тост",
            Script;
        public UserRights Right = UserRights.All;
        private bool Runing;
        public ComandEvent()
        {

        }
        public void Invoke(MessageEventArgs e)
        {
            while (Runing)
                Thread.Sleep(100);
            Runing = true;
            string scripd = Script;
            if (scripd.Contains("%"))
            {

                if (scripd.Contains("%TEXT%"))
                    scripd = scripd.Replace("%TEXT%", e.Message.Replace("\n", "").Replace(">", "").Split(new char[] { ' ' }, 2).Last().Trim());
                if (scripd.Contains("%NICK%"))
                    scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", "").Trim());
                //if (scripd.Contains("%TEXT%"))
                //  scripd = scripd.Replace("%TEXT%", e.Text.Replace("\n", ""));
            }
            ScriptLanguage.RunScript(scripd);
            Runing = false;
        }
    }
    public class DonationEvent
    {
        public int MinLimit = 0, MaxLimit = 100;
        public string Script, Name = "Новый";
        private bool Runing;
        public DonationEvent()
        {

        }
        public bool Check(DonationEventArgs e)
        {
            return e.Amount >= MinLimit && (MaxLimit == 0 || e.Amount <= MaxLimit);
        }
        public void Invoke(DonationEventArgs e)
        {
            while (Runing)
                Thread.Sleep(100);
            Runing = true;
            string scripd = Script;
            if (scripd.Contains("%"))
            {
                if (scripd.Contains("%TEXT%"))
                    scripd = scripd.Replace("%TEXT%", e.Message.Replace("\n", "").Trim());
                if (scripd.Contains("%NICK%"))
                    scripd = scripd.Replace("%NICK%", e.NickName.Replace("\n", "").Trim());
                if (scripd.Contains("%AMOUNT%"))
                    scripd = scripd.Replace("%AMOUNT%", e.Amount.ToString());
                if (scripd.Contains("%CURRENCY%"))
                    scripd = scripd.Replace("%CURRENCY%", e.Currency.Replace("\n", "").Trim());
            }
            ScriptLanguage.RunScript(scripd);
            Runing = false;
        }
    }
}
