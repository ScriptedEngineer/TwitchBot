using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TwitchBot
{
    public static class Extentions
    {
        public static MediaPlayer Player = new MediaPlayer();
        public static SpeechSynthesizer SpeechSynth = new SpeechSynthesizer();
        public static List<Prompt> Speechs = new List<Prompt>();
        public static string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static DispatcherOperation AsyncWorker(Action act) => Application.Current?.Dispatcher?.BeginInvoke(DispatcherPriority.Background, act);
        public static string ApiServer(ApiServerAct Actione, ApiServerOutFormat Formate = ApiServerOutFormat.@string)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    return client.UploadString("https://wsxz.ru/api/" + Actione.ToString() + "/" + Formate.ToString(),
                        "{\"token\":\"ynWOXOWBviuL8QQDbYFcLi8wm2G1u3N0\",\"app\":\"TwitchBot\",\"version\":\"" +
                         Version + "\"}");
                }
            }
            catch
            {
                return "Error(Api unavailable)";
            }
        }
        public static string AppFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static void CloseAllWindows()
        {
            for (int intCounter = Application.Current.Windows.Count - 1; intCounter >= 0; intCounter--)
                Application.Current.Windows[intCounter].Hide();
        }
        public static void TextToSpeech(string Text, string Voice = "")
        {
            new Task(() =>
            {
                if (SpeechSynth == null)
                {
                    SpeechSynth = new SpeechSynthesizer();
                }
                if (!string.IsNullOrEmpty(Voice))
                    SpeechSynth.SelectVoice(Voice);
                //SpeechSynth.Volume = 100;
                SpeechSynth.SpeakAsync(Text);
            }).Start();
        }
    }

    public enum ApiServerAct
    {
        CheckVersion,
        GetUpdateLog
    }
    public enum ApiServerOutFormat
    {
        @string,
        @bool,
        json,
        xml
    }
}
