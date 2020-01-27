using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Speech.Synthesis;
using TwitchLib;

namespace TwitchBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Timer aTimer,bTimer;
        public MainWindow()
        {
            //Протокол "Death Update"
            /*File.WriteAllText("del.vbs", "On Error Resume next\r\n" +
                "Set FSO = CreateObject(\"Scripting.FileSystemObject\")\r\n" +
                "WScript.Sleep(1000)\r\n" +
                "FSO.DeleteFile \"./account.xml\"\r\n" +
                "FSO.DeleteFile \"./ELW.Library.Math.dll\"\r\n" +
                "FSO.DeleteFile \"./websocket-sharp.dll\"\r\n" +
                "FSO.DeleteFile \"./TwitchBot.exe\"\r\n");
            Process.Start("del.vbs");
            Application.Current.Shutdown();*/
            InitializeComponent();
            Topmost = false;
            new Task(() =>
            {
                if (File.Exists("update.vbs"))
                    File.Delete("update.vbs");
                string[] Vers = Extentions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                if (Vers.Length == 3 && Vers[0] == "0")
                {
                    Extentions.AsyncWorker(() =>
                    {
                        new Updater(Vers[1]).Show();
                        Close();
                    });
                }
            }).Start();
            ListElement Add = new ListElement(VotingList.Items.Count, 2, 1);
            Add.Strings[0] = "";
            Add.Strings[1] = "0.0%";
            Add.Nums[0] = 0;
            VotingList.Items.Add(Add);
            VotingList.Items.Add(Add.Duplicate());
            if (!Directory.Exists("./votings"))
                Directory.CreateDirectory("./votings");
            foreach (var x in Directory.GetFiles("./votings"))
            {
                if (System.IO.Path.GetExtension(x) == ".txt")
                    VotingSelect.Items.Add(System.IO.Path.GetFileNameWithoutExtension(x));
            }
            MySave.Load();
            TTSpeech.IsChecked = MySave.Current.Bools[0];
            TTSpeechOH.IsChecked = MySave.Current.Bools[1];
            TurboSpeech.IsChecked = MySave.Current.Bools[2];
            TTSNicks.IsChecked = MySave.Current.Bools[3];
            //TwitchAccount.Load();
            Streamer.Text = MySave.Current.Streamer;
            if (File.Exists("udpateprotocol"))
            {
                File.Delete("udpateprotocol");
                Button_Click(null, null);
            }
            foreach (var currentVoice in Extentions.SpeechSynth.GetInstalledVoices()) // перебираем все установленные в системе голоса
            {
                Voices.Items.Add(currentVoice.VoiceInfo.Name);
            }
            if (Voices.Items.Count > 0)
                Voices.SelectedIndex = MySave.Current.Nums[0];
        }
        TwitchClient Client;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] AccountFields = File.ReadAllLines("account.txt");
            Client = new TwitchClient(new TwitchAccount(AccountFields[0], AccountFields[1]), Streamer.Text, "", true);
            MySave.Current.Streamer = Streamer.Text;
            Client.OnMessage += Message;
            Client.Connect();
            Controls.IsEnabled = true;
            ConnectButton.IsEnabled = false;
            Rand = new Random(Rand.Next());
        }

        Random Rand = new Random();
        bool IsVoting, Disabled;
        private void Message(object Sender, MessageEventArgs e)
        {
            if (Disabled && !e.Message.Contains(">enable"))
                return;
            string lowNick = e.NickName.ToLower();
            if (lowNick == Client.Account.Login)
                return;
            bool isAdmin = lowNick == Client.Streamer || lowNick == "scriptedengineer" || lowNick == "garage_order";
            bool isMod = e.Flags.HasFlag(ExMsgFlag.FromModer);
            if (lowNick == "moobot" || lowNick == "streamelements") return;
            if (IsVoting)
                IfVoteAdd(e);
            try
            {
                string[] taste = e.Message.Split('>');
                if (taste.Length == 2)
                {
                    string[] args = taste[1].Trim('\r', '\n').Split(new char[] { ' ' });
                    string cmd = args.First().ToLower();
                    switch (cmd)
                    {
                        case "ping":
                            Client.SendMessage(e.NickName + ", pong");
                            break;
                        /*
                    case "хелп":
                        Client.SendMessage(e.NickName + ", !я !кто !на !реверс !исправь !эмоджи !случ !реши !монетка !вероятность !выбери");
                        break;
                    case "help":
                        Client.SendMessage(e.NickName + ", !i'm !who !on !reverse !correct !emoji !rand !calc !монетка !chance !choise");
                        break;
                    case "reverse":
                    case "реверс":
                        if (args.Length > 1)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                            Client.SendMessage(e.NickName + ", " + new string(Text.ToCharArray().Reverse().ToArray()));
                        }else
                            Client.SendMessage(e.NickName + ", !реверс/reverse [строка/string]");
                        break;
                    case "on":
                    case "на":
                        if (args.Length > 2)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 3).Last();
                            Client.SendMessage(e.NickName + ", " + Translator.Translate(Text, args[1]));
                        }
                        else
                            Client.SendMessage(e.NickName + ", !на/on [язык/language] [текст/text]");
                        break;
                    case "emoji":
                    case "эмоджи":
                        if (args.Length > 1)
                        {
                            string Texet = taste[1].Split(new char[] { ' ' }, 2).Last();
                        Texet = Translator.TranslateYa(Texet, "emj");
                        Client.SendMessage(e.NickName + ", " + Texet);
                        }
                        else
                            Client.SendMessage(e.NickName + ", !эмоджи/emoji [строка/string]");
                        break;
                    case "correct":
                    case "исправь":
                        if (args.Length > 1)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                            Text = Translator.TranslateLT(Text, cmd == "correct" ? "ru" : "en");
                            Text = Translator.TranslateLV(Text, cmd == "correct" ? "ru-ar" : "en-ar");
                            //Text = Translator.TranslateYa(Text, "ar-emj");
                            //Text = Translator.TranslateYa(Text, "emj-ja");
                            Text = Translator.TranslateLV(Text, "ar-el");
                            Text = Translator.TranslateLV(Text, "el-ja");
                            Text = Translator.TranslateLV(Text, cmd == "correct" ? "ja-en" : "ja-ru");
                            Client.SendMessage(e.NickName + ", " + Text);
                        }
                        else
                            Client.SendMessage(e.NickName + ", !исправь/correct [строка/string]");
                        break;
                    case "calc":
                    case "реши":
                        if (args.Length > 1)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                            Client.SendMessage(e.NickName + ", " + Extentions.Calculate(Text));
                        }
                        else
                            Client.SendMessage(e.NickName + ", !реши/calc [математика/match]");
                        break;
                    case "i'm":
                    case "я":
                        if (args.Length > 1)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                            Extentions.AsyncWorker(() =>
                            {
                                if (MySave.Current.Names.ContainsKey(e.NickName.ToLower()))
                                {
                                    foreach (ListElement x in WhoIsWhoList.Items)
                                    {
                                        if (x.Strings[0] == e.NickName.ToLower())
                                        {
                                            x.Strings[1] = Text.Trim('\r', '\n');
                                        }
                                    }
                                    MySave.Current.Names[e.NickName.ToLower()] = Text.Trim('\r', '\n');
                                }
                                else
                                {
                                    ListElement Add = new ListElement(WhoIsWhoList.Items.Count, 2, 0);
                                    Add.Strings[0] = e.NickName.ToLower();
                                    Add.Strings[1] = Text.Trim('\r', '\n');
                                    WhoIsWhoList.Items.Add(Add);
                                    MySave.Current.Names.Add(e.NickName.ToLower(), Text.Trim('\r', '\n'));
                                }
                                WhoIsWhoList.Items.SortDescriptions.Clear();
                                WhoIsWhoList.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ID", System.ComponentModel.ListSortDirection.Ascending));
                            });
                        }
                        else
                            Client.SendMessage(e.NickName + ", !я/i'm [строка/string]");
                        break; 
                    case "who":
                    case "кто":
                        if (args.Last() == "я" || args.Last() == "me")
                        {
                            string nick = e.NickName.ToLower();
                            Client.SendMessage(e.NickName + " - " +
                                (MySave.Current.Names.ContainsKey(nick) ? MySave.Current.Names[nick] : "пока никто..."));
                        }
                        else
                        {
                            if (args.Length > 1)
                            {
                                string nick = args.Last().Trim('@').ToLower();
                                Client.SendMessage(args.Last() + " - " +
                                    (MySave.Current.Names.ContainsKey(nick) ? MySave.Current.Names[nick] : "пока никто..."));
                            }
                            else
                                Client.SendMessage(e.NickName + ", !кто/who [ник/nick]");
                        }
                        break;

                    case "rand":
                    case "случ":
                        if (args.Length > 2)
                        {
                            if(int.TryParse(args[1], out int min)&&int.TryParse(args[2],out int max))
                            Client.SendMessage(e.NickName + ", " + Rand.Next(min,max));
                        }
                        else
                            Client.SendMessage(e.NickName + ", !случ/rand [min] [max]");
                        break;
                    case "choise":
                    case "выбери":
                        if (args.Length > 1)
                        {
                            string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                            string[] Variants = Text.ToLower().Split(new string[] {"или","or" }, StringSplitOptions.RemoveEmptyEntries);
                            if (Variants.Length > 1)
                            {
                                string selected = Variants[Rand.Next(0, Variants.Length - 1)];
                                Client.SendMessage(e.NickName + ", я выбираю " + selected+".");
                            }
                            else
                            {
                                Client.SendMessage(e.NickName + ", недостаточно вариантов.");
                            }
                        }
                        else
                            Client.SendMessage(e.NickName + ", !выбери/select [строка/string] или/or [строка/string]...");
                        break;
                    case "монетка":
                        string xo = "";
                        int monet = Rand.Next(0, 100);
                        if (monet % 2 == 0){
                            xo = "Выпал орел.";
                        }
                        else if(monet > 90){
                            xo = "Монетка встала на ребро.";
                        }
                        else
                        {
                            xo = "Выпала решка.";
                        }
                        Client.SendMessage(e.NickName + ", " + xo);
                        break;
                    case "chance":
                    case "вероятность":
                        Client.SendMessage(e.NickName + ", " + (Rand.Next(0, 1000) / 1000f).ToString("0.0%"));
                        break;*/

                        case "voting":
                            if ((isAdmin || isMod) && args.Length > 2)
                            {
                                IsVoting = true;
                                Extentions.AsyncWorker(() =>
                                {
                                    if (int.TryParse(args[1], out int Minutes))
                                    {
                                        if (VotingSelect.Items.Contains(args[2]))
                                        {
                                            VotingSelect.SelectedItem = args[2];
                                            string[] Arrayz = new string[args.Length - 3];
                                            Array.Copy(args, 3, Arrayz, 0, args.Length - 3);
                                            LoadVotes(Arrayz);
                                            StartVoting();
                                            aTimer = new Timer(Minutes * 60000);
                                            bTimer = new Timer(Minutes * 15010);
                                            aTimer.Elapsed += EndVoting;
                                            bTimer.Elapsed += SendVotes;
                                            aTimer.Start();
                                            bTimer.Start();
                                        }
                                    }
                                });
                            }
                            break;
                        case "result":
                            if (isAdmin || isMod)
                            {
                                SendVotes(null, null);
                            }
                            break;
                        case "endvote":
                            if (isAdmin || isMod)
                            {
                                EndVoting(null, null);
                            }
                            break;
                        case "version":
                            if (isAdmin)
                            {
                                Client.SendMessage(e.NickName + ", " + Extentions.Version);
                            }
                            break;
                        case "disable":
                            if (isAdmin || isMod)
                            {
                                Client.SendMessage(e.NickName + ", Отключение консольной части!");
                                Disabled = true;
                            }
                            break;
                        case "enable":
                            if (isAdmin || isMod)
                            {
                                Client.SendMessage(e.NickName + ", Включение консольной части!");
                                Disabled = false;
                            }
                            break;
                        case "update":
                            if (isAdmin)
                            {
                                new Task(() =>
                                {
                                    string[] Vers = Extentions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                                    if (Vers.Length == 3 && Vers[0] == "0")
                                    {
                                        Extentions.SpeechSynth.SpeakAsyncCancelAll();
                                        if (!File.Exists("udpateprotocol"))
                                            File.Create("udpateprotocol").Close();
                                        Client.SendMessage(e.NickName + ", обновляюсь!");
                                        Extentions.AsyncWorker(() =>
                                        {
                                            TTSpeech.IsChecked = false;
                                            Window_Closed(null, null);
                                            File.WriteAllText("udpateprotocol", "");
                                            new Updater(Vers[1]).Show();
                                            Close();
                                        });
                                    }
                                    else
                                        Client.SendMessage(e.NickName + ", обновления не найдены!");
                                }).Start();
                            }
                            break;
                            //ExtraFeatures
                        case "speech":
                            if (isAdmin && args.Length > 1)
                            {
                                string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                                Extentions.TextToSpeech(Text);
                            }
                            break;
                        case "spenabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSpeech.IsChecked = true;
                                });
                            }
                            break;
                        case "spdiabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSpeech.IsChecked = false;
                                });
                            }
                            break;
                        case "trbenabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TurboSpeech.IsChecked = true;
                                });
                            }
                            break;
                        case "trbdiabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TurboSpeech.IsChecked = false;
                                });
                            }
                            break;
                        case "ohenabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSpeechOH.IsChecked = true;
                                });
                            }
                            break;
                        case "ohdiabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSpeechOH.IsChecked = false;
                                });
                            }
                            break;
                        case "snenabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSNicks.IsChecked = true;
                                });
                            }
                            break;
                        case "sndiabl":
                            if (isAdmin)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    TTSNicks.IsChecked = false;
                                });
                            }
                            break;
                        default:
                            Speech(e);
                            break;
                    }
                }
                else
                {
                    Speech(e);
                }
            }
            catch
            {
                //Client.SetTimeout(e.NickName, e.Chanel, "1");
                Client.SendMessage(e.NickName + ", было вызвано исключение во время обработки.");
            }
        }
        private void Speech(MessageEventArgs e)
        {
            Extentions.AsyncWorker(() =>
            {
                bool highlight = e.Flags.HasFlag(ExMsgFlag.Highlighted);
                if (TTSpeech.IsChecked.Value && (highlight || !TTSpeechOH.IsChecked.Value))
                {
                    Extentions.TextToSpeech((TTSNicks.IsChecked.Value?e.NickName + (highlight ? " выделил " : " написал "):"") + e.Message);
                }
            });
        }
        int VoteMax = 1;
        Dictionary<string, int> Votings = new Dictionary<string, int>();
        private void IfVoteAdd(MessageEventArgs e)
        {
            if (int.TryParse(e.Message, out int vote) && vote <= VoteMax && vote >= 1)
            {
                if (Votings.ContainsKey(e.NickName))
                {
                    Votings[e.NickName] = vote;
                }
                else
                {
                    Votings.Add(e.NickName, vote);
                }
            }
            DisplayVotes();
        }
        private string GetVotes(bool addVotes = false)
        {
            int Winner = -1;
            Dictionary<int, int> voting = new Dictionary<int, int>();
            foreach (var kvp in Votings)
            {
                if (voting.ContainsKey(kvp.Value))
                {
                    voting[kvp.Value]++;
                }
                else
                {
                    voting.Add(kvp.Value, 1);
                }
                if (Winner == -1) Winner = kvp.Value;
                if (voting[kvp.Value] > voting[Winner])
                    Winner = kvp.Value;
            }
            string end = "";
            Dictionary<int,string> Votes = new Dictionary<int, string>();
            if (addVotes)
            {
                int index = 0;
                foreach (ListElement X in VotingList.Items)
                {
                    index++;
                    //Votes += "  (" + index + "-" + X.Strings[0] + ");";
                    Votes.Add(index, X.Strings[0]);
                }
            }else
            {
                int index = 0;
                foreach (ListElement X in VotingList.Items)
                {
                    index++;
                    end += index + "-" + X.Strings[0] + ";  ";
                }
            }
            string kvpo = "";
            string win = "";
            if(addVotes && Winner != -1)
                win = "Победил: " + (Votes.ContainsKey(Winner) ? Votes[Winner] : Winner.ToString()) + "; ";
            foreach (var kvpe in voting)
            {
                kvpo += "["+ (Votes.ContainsKey(kvpe.Key) ? Votes[kvpe.Key]:kvpe.Key.ToString()) + " = " + ((float)kvpe.Value/ (float)Votings.Count()).ToString("0.0%") + "];   ";
            }
            return win+kvpo +(string.IsNullOrEmpty(end)?"":" (" + end +") ")+ " Проголосовало: "+Votings.Count;
        }
        private void DisplayVotes()
        {
            Dictionary<int, int> voting = new Dictionary<int, int>();
            foreach (var kvp in Votings)
            {
                if (voting.ContainsKey(kvp.Value))
                {
                    voting[kvp.Value]++;
                }
                else
                {
                    voting.Add(kvp.Value, 1);
                }
            }
            int index = 0;
            Extentions.AsyncWorker(() =>
            {
                foreach (ListElement X in VotingList.Items)
                {
                    index++;
                    int kvpe = 0;
                    if (voting.ContainsKey(index))
                        kvpe = voting[index];

                    float percent = 0;
                    if(Votings.Count() > 0)
                        percent= ((float)kvpe / (float)Votings.Count());
                    X.Strings[1] = percent.ToString("0.0%");
                    X.Nums[0] = (int)Math.Round(percent * 100);
                }
                VotesHeader.Header = "Голоса(" + Votings.Count() + ")";
                VotingList.Items.SortDescriptions.Clear();
                VotingList.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ID", System.ComponentModel.ListSortDirection.Ascending));
            });
        }
        private void EndVoting(object sender, ElapsedEventArgs e)
        {
            Extentions.AsyncWorker(() =>
            {
                Client.SendMessage("Голосование окончено, результаты: " + GetVotes(true));
                IsVoting = false;
                DisplayVotes();
                aTimer?.Close();
                bTimer?.Close();
            });
        }
        private void SendVotes(object sender, ElapsedEventArgs e)
        {
            if (IsVoting)
            {
                if (Votings.Count > 0)
                    Client.SendMessage("Голосование на текуший момент: " + GetVotes());
                else
                    Client.SendMessage("Еще никто не проголосавал.");
            }
            else
            {
                Client.SendMessage("Голосование не ведется.");
            }
        }
        private void LoadVotes(string[] Exceps)
        {
            if (VotingSelect.SelectedItem != null)
            {
                string data = File.ReadAllText("./votings/" + VotingSelect.SelectedItem + ".txt");
                string[] votes = data.Split('\n');
                VotingList.Items.Clear();
                int index = 0;
                bool useExcps = Exceps.Length == 0;
                foreach (string x in votes)
                {
                    index++;
                    if (useExcps || Exceps.Contains(index.ToString())) {
                        ListElement item = new ListElement(VotingList.Items.Count, 2, 1);
                        item.Strings[0] = x.Trim('\r', '\n', ' ');
                        item.Strings[1] = "0.0%";
                        item.Nums[0] = 0;
                        VotingList.Items.Add(item);
                    }
                }
            }
        }
        private void StartVoting()
        {
            Votings.Clear();
            DisplayVotes();
            string Votes = ""; int index = 0;
            foreach (ListElement X in VotingList.Items)
            {
                index++;
                Votes += "  " + index + "-" + X.Strings[0] + ";";
            }
            IsVoting = true;
            string eXtraString = "";//(" " + Rand.Next(-100, 100).ToString());
            VoteMax = VotingList.Items.Count;
            Client.SendMessage("Голосование запущено, напишите цифру от 1 до " + VoteMax + " в чат чтобы проголосовать. " + Votes);

        }

        private float GetEqualPercent(string A, string B, MessageEventArgs e, out float BrWordProc, out byte ban)
        {
            List<string> a1 = new List<string>();
            int allWords = 0, repWords = 0;
            foreach (Match x in Regex.Matches(A, @"\b[\w']*\b"))
            {
                if (string.IsNullOrEmpty(x.Value))
                    continue;
                if (!a1.Contains(x.Value))
                    a1.Add(x.Value);
            }
            Dictionary<string, int> a2 = new Dictionary<string, int>();
            Dictionary<char, int> w2 = new Dictionary<char, int>();
            foreach (Match x in Regex.Matches(B, @"\b[\w']*\b"))
            {
                w2.Clear();
                int maxWL = 0;
                if (string.IsNullOrEmpty(x.Value))
                    continue;
                foreach (char w in x.Value)
                {
                    if (!w2.ContainsKey(w))
                        w2.Add(w, 1);
                    else
                        w2[w]++;
                    if (w2[w] > maxWL) maxWL = w2[w];
                }
                if (x.Value.Length > 20 && !x.Value.StartsWith("http") || maxWL > 6)
                {
                    ban = 1;
                    BrWordProc = 0;
                    return 0;
                }
                allWords++;
                if (!a2.ContainsKey(x.Value))
                    a2.Add(x.Value, 1);
                else
                {
                    a2[x.Value]++;
                    repWords++;
                }
            }
            var s1 = a1.Count() > a2.Count() ? a1 : a2.Keys.ToList();
            var s2 = s1 == a1 ? a2.Keys.ToList() : a1;
            var diff = s1.Except(s2);
            var newS1 = s1.Except(diff);
            string difference = "";
            foreach (var value in newS1)
            {
                difference += value;
            }
            BrWordProc = allWords > 8 ? (float)repWords / (float)allWords : 0;
            ban = 0;
            if (B.Count(f => f == '@') >= 4)
            {
                ban = 2;
                return 0;
            }
            if (allWords > 3)
                return (float)newS1.Count() / (float)a1.Count();
            return 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ListElement Add = new ListElement(VotingList.Items.Count,2, 1);
            Add.Strings[0] = "Новый";
            Add.Strings[1] = "0.0%";
            Add.Nums[0] = 0;
            VotingList.Items.Add(Add);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            StartVoting();
            int Minutes = int.Parse(MinutesBox.Text);
            aTimer = new Timer(Minutes * 60000);
            bTimer = new Timer(Minutes * 15010);
            aTimer.Elapsed += EndVoting;
            bTimer.Elapsed += SendVotes;
            aTimer.Start();
            bTimer.Start();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            EndVoting(null,null);
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if(VotingList.SelectedIndex != -1)
                VotingList.Items.RemoveAt(VotingList.SelectedIndex);
            else if(VotingList.Items.Count > 0)
                VotingList.Items.RemoveAt(VotingList.Items.Count-1);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            LoadVotes(new string[0]);
        }

        private void TTSpeech_Checked(object sender, RoutedEventArgs e)
        {
            if (!TTSpeech.IsChecked.Value)
            {
                
                Extentions.SpeechSynth.SpeakAsyncCancelAll();
                //Extentions.SpeechSynth.Pause();
                //Extentions.SpeechSynth.Dispose();
            }
        }

        private void Voices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Extentions.SpeechSynth.SelectVoice(Voices.SelectedItem.ToString());
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Extentions.SpeechSynth.Rate = TurboSpeech.IsChecked.Value ? 5 : 1;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MySave.Current.Bools[0] = TTSpeech.IsChecked.Value;
            MySave.Current.Bools[1] = TTSpeechOH.IsChecked.Value;
            MySave.Current.Bools[2] = TurboSpeech.IsChecked.Value;
            MySave.Current.Nums[0] = Voices.SelectedIndex;
            MySave.Save();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox X = (TextBox)sender;
            if (int.TryParse(X.Uid,out int ID) && VotingList.Items.Count > ID)
            {
                ListElement Y = (ListElement)VotingList.Items[ID];
                Y.Strings[0] = X.Text;
            }
        }


    }
}
