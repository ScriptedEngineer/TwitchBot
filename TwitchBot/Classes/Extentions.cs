using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Media;
using System.Net;
using TwitchLib;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;

namespace TwitchBot
{
    public static class Extentions
    {
        public static MediaPlayer Player = new MediaPlayer();
       // public static SoundPlayer WavePlayer = new SoundPlayer();
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
                         Version + "\",\"streamer\":\"" + MySave.Current.Streamer + "\"}");
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

        private static float RateToSpeed()
        {
            if (SpeechSynth.Rate == 0)
                return 1;
            else if(SpeechSynth.Rate > 0)
            {
                return (float)(1+(SpeechSynth.Rate * 0.1));
            }
            else
            {
                return (float)(1-((SpeechSynth.Rate*-1d)/20d));
            }
        }

        public static void GetTrueTTSReady(string Text, string Settings = @"""voice"":""alena"",""emotion"":""neutral""")
        {
            string Setts = @"{
""message"":""" + Text.Replace(@"""",@".").Replace(@"\",@"\\") + @""",
""language"":""ru-RU"",
""speed"":" + RateToSpeed().ToString().Replace(",",".") + @",
" + Settings + @",
""format"":""lpcm""
}";

            byte[] byteArray = Encoding.UTF8.GetBytes(Setts);
            HttpWebRequest reqGetUser = (HttpWebRequest)WebRequest.Create("https://cloud.yandex.ru/api/speechkit/tts");
            reqGetUser.Accept = "application/json";
            reqGetUser.ContentType = "application/json;charset=UTF-8";
            reqGetUser.Method = "POST";
            reqGetUser.Headers["x-csrf-token"] = MySave.Current.YPT;
            reqGetUser.Headers["Cookie"] = @"XSRF-TOKEN="+HttpUtility.UrlEncode(MySave.Current.YPT);
            reqGetUser.ContentLength = byteArray.Length;
            Stream dataStream = reqGetUser.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response;
            try
            {
                
                response = reqGetUser.GetResponse();
                Stream receiveStream = response.GetResponseStream();
                if (File.Exists("YAPI.wav")) File.Delete("YAPI.wav");
                using (var fileStream = File.Create("YAPI.wav"))
                {
                    WriteWavHeader(fileStream, false, 1, 16, 48000, -1);
                    CopyStream(receiveStream, fileStream);
                }
                TrueTTSReady = true;
            }
            catch (WebException ex)
            {
                Console.WriteLine(Web.GetResponse(ex.Response));
                try
                {
                    Match MTH = Regex.Match(ex.Response.Headers["Set-Cookie"], @"XSRF-TOKEN=([^;]*);");
                    if (MTH.Success)
                    {
                        MySave.Current.YPT = HttpUtility.UrlDecode(MTH.Groups[1].Value);
                    }
                }
                catch(Exception e)
                {

                }
                TrueTTSReady = false;
            }
        }
        static bool TrueTTSReady = false;
        public static void TrueTTS(string Text, string Voice = "")
        {
            if (!TrueTTSReady)
            {
                TextToSpeech(Text, Voice);
                return;
            }
            AsyncWorker(() =>
            {
                string path = Path.GetDirectoryName(AppFile) + "\\YAPI.wav";
                //Uri File = new Uri(path, UriKind.Absolute);
                /*if (!MySave.Current.Bools[0])
                {
                    return;
                }*/
                Player.Open(new Uri("F:/lol.mp3"));
                Player.Open(new Uri(path, UriKind.Absolute));
                Player.Play();
            });
            Thread.Sleep(1000);
            Thread.Sleep(MainWindow.CurrentW.MediaDurationMs);
            AsyncWorker(() =>
            {
                Player.Close();
            });
            TrueTTSReady = false;
        }
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }
        public static void WriteWavHeader(Stream stream, bool isFloatingPoint, ushort channelCount, ushort bitDepth, int sampleRate, int totalSampleCount)
        {
            stream.Position = 0;

            // RIFF header.
            // Chunk ID.
            stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);

            // Chunk size.
            stream.Write(BitConverter.GetBytes(((bitDepth / 8) * totalSampleCount) + 36), 0, 4);

            // Format.
            stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);



            // Sub-chunk 1.
            // Sub-chunk 1 ID.
            stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);

            // Sub-chunk 1 size.
            stream.Write(BitConverter.GetBytes(16), 0, 4);

            // Audio format (floating point (3) or PCM (1)). Any other format indicates compression.
            stream.Write(BitConverter.GetBytes((ushort)(isFloatingPoint ? 3 : 1)), 0, 2);

            // Channels.
            stream.Write(BitConverter.GetBytes(channelCount), 0, 2);

            // Sample rate.
            stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);

            // Bytes rate.
            stream.Write(BitConverter.GetBytes(sampleRate * channelCount * (bitDepth / 8)), 0, 4);

            // Block align.
            stream.Write(BitConverter.GetBytes((ushort)channelCount * (bitDepth / 8)), 0, 2);

            // Bits per sample.
            stream.Write(BitConverter.GetBytes(bitDepth), 0, 2);



            // Sub-chunk 2.
            // Sub-chunk 2 ID.
            stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);

            // Sub-chunk 2 size.
            stream.Write(BitConverter.GetBytes((bitDepth / 8) * totalSampleCount), 0, 4);
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
