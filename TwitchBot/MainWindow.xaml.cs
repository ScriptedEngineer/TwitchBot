using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Speech.Synthesis;
using TwitchLib;
using System.Threading;
using System.Xml.Serialization;
using Screen = System.Windows.Forms.Screen;
using TwitchBot.Classes;
using System.Net.Http.Headers;

namespace TwitchBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static System.Timers.Timer aTimer, bTimer;
        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
        public int MediaDurationMs = 0;
        bool Tray = false;
        Thread SpeechTask;
        OBSWebSock OBSRemote;
        DonationAlerts DonAlert;
        static public MainWindow CurrentW;
        public MainWindow()
        {
            CurrentW = this;
            MySave.Load();
            //Инициализация трей иконки
            ni.Icon = Properties.Resources.icon;
            ni.Visible = true;
            EventHandler OpenShow = (sndr, args) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Tray = false;
            };
            ni.DoubleClick += OpenShow;
            ni.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[] { 
            new System.Windows.Forms.MenuItem("Развернуть",OpenShow),
            new System.Windows.Forms.MenuItem("Закрыть",(sender,EventArgs)=>Close())});


            //Инициализация компонентов GUI
            InitializeComponent();
            MaxWidth = 550;
            MinWidth = 350;
            MaxHeight = 95;
            MinHeight = 95;
            Width = 400;
            Height = 95;
            ConnectButton.Visibility = Visibility.Visible;
            Streamer.Visibility = Visibility.Visible;
            Thanks.Visibility = Visibility.Collapsed;
            Controls.Visibility = Visibility.Collapsed;
            Topmost = false;
            if (!File.Exists("./votings/Текущий.txt"))
            {
                ListElement Add = new ListElement(VotingList.Items.Count, 2, 1);
                Add.Strings[0] = "";
                Add.Strings[1] = "0.0%";
                Add.Nums[0] = 0;
                VotingList.Items.Add(Add);
                VotingList.Items.Add(Add.Duplicate());
            }
            else
            {
                LoadVotes(new string[0]);
            }

            //Автоматические обновления
            new Task(() =>
            {
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

            //Инициализация заготовок
            if (!Directory.Exists("./votings"))
                Directory.CreateDirectory("./votings");
            foreach (var x in Directory.GetFiles("./votings"))
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(x);
                if (filename == "Текущий")
                    continue;
                if (System.IO.Path.GetExtension(x) == ".txt")
                    VotingSelect.Items.Add(filename);
            }

            //Применение настроек
            TTSpeech.IsChecked = MySave.Current.Bools[0];
            TTSpeechOH.IsChecked = MySave.Current.Bools[1];
            TTSNotifyUse.IsChecked = MySave.Current.Bools[2];
            TTSNicks.IsChecked = MySave.Current.Bools[3];
            DontTTS.IsChecked = MySave.Current.Bools[4];
            MinimizeToTray.IsChecked = MySave.Current.Bools[5];
            Filtred.IsChecked = MySave.Current.Bools[6];
            OBSRemEn.IsChecked = MySave.Current.Bools[7];
            UseYA.IsChecked = MySave.Current.Bools[8];
            DATTSEnable.IsChecked = MySave.Current.Bools[9];
            DANotify.IsChecked = MySave.Current.Bools[10];
            DTTSNicks.IsChecked = MySave.Current.Bools[11];
            DTTSAmount.IsChecked = MySave.Current.Bools[12];
            BadWords.Text = MySave.Current.BadWords;
            Censor.Text = MySave.Current.Censor;
            Streamer.Text = MySave.Current.Streamer;
            CustomRewardID.Text = MySave.Current.TTSCRID;
            RewardName.Text = MySave.Current.TTSCRTitle;
            OBS_port.Text = MySave.Current.OBSWSPort;
            OBSRmPass.Password = MySave.Current.OBSWSPass;
            Voices_TTTS.SelectedIndex = (int)MySave.Current.YPV;
            Voices_TTTS_Copy.SelectedIndex = (int)MySave.Current.DYPV;
            TTSNotifyLabel.Content = System.IO.Path.GetFileName(MySave.Current.TTSNTFL);
            TTSNotifyLabel_Copy.Content = System.IO.Path.GetFileName(MySave.Current.DTTSNTFL);
            foreach (var currentVoice in Extentions.SpeechSynth.GetInstalledVoices(Thread.CurrentThread.CurrentCulture)) // перебираем все установленные в системе голоса
            {
                Voices.Items.Add(currentVoice.VoiceInfo.Name);
            }
            try
            {
                if (Voices.Items.Count > 0)
                    Voices.SelectedIndex = MySave.Current.Nums[0];
            }
            catch
            {
                Voices.SelectedIndex = 0;
            }
            Volume.Value = MySave.Current.Nums[4];
            MaxSymbols.Text = MySave.Current.Nums[3].ToString();
            int num = MySave.Current.Nums[2];
            WaitPlease.Text = MySave.Current.Nums[6].ToString();
            SynthSpeed.Value = num;
            SpeedLabel.Content = $"Скорость ({num}) [{Extentions.RateToSpeed()}x]";
            switch (MySave.Current.Nums[1])
            {
                case 0:
                    AllChat.IsChecked = true;
                    break;
                case 1:
                    TTSpeechOH.IsChecked = true;
                    break;
                case 2:
                    CustomReward.IsChecked = true;
                    break;
            }
            SwitcherKey = new WinHotKey(MySave.Current.Hotkey, MySave.Current.HotkeyModifier, AcSwitch);
            HotKey.Text = (MySave.Current.HotkeyModifier == KeyModifier.None ? "" : MySave.Current.HotkeyModifier.ToString() + "+") + MySave.Current.Hotkey;
            Extentions.Player.MediaOpened += (object s, EventArgs ex) => { MediaDurationMs = (int)(Extentions.Player.NaturalDuration.HasTimeSpan? Extentions.Player.NaturalDuration.TimeSpan.TotalMilliseconds:0); };
            VersionLabel.Content = "v" + Extentions.Version;
            LoadEvents();

            MyCensor.Init();

            //Постupdate инициализация
            if (File.Exists("udpateprotocol"))
            {

                if (File.ReadAllText("udpateprotocol") == "True")
                {
                    Tray = true;
                    Hide();
                }
                ConnectButton.IsEnabled = false;
                ConnectButton.Content = "Автоматически";
                new Task(() =>
                {
                    Thread.Sleep(2000);
                    File.Delete("udpateprotocol");
                    Extentions.AsyncWorker(() =>
                    {
                        Button_Click(null, null);
                    });
                }).Start();
            }

            //Web сервер визуалки
            WebServer = new WebServer("http://localhost:8190/");
            WebServer.Run();

            //Авторизация
            if (!File.Exists("account.txt"))
            {
                Process.Start("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=v1wv59aw5a8w2reoyq1i5j6mwb1ixm&redirect_uri=http://localhost:8190/twitchcode&scope=chat:edit%20chat:read");
                while (!File.Exists("account.txt"))
                {
                    Thread.Sleep(500);
                }
            }
            string[] AccountFields = File.ReadAllLines("account.txt");
            Account = new TwitchAccount(AccountFields[0], AccountFields[1]);

            if (File.Exists("da.txt"))
            {
                try
                {
                    string[] dafi = File.ReadAllText("da.txt").Split('\n');
                    DonAlert = new DonationAlerts(dafi[0], dafi[1]);
                    DonAlert.OnDonation += Donation;
                    DAConnect.IsEnabled = false;
                }
                catch
                {

                }
            }

            //Параметры командной строки
            string[] argsv = Environment.GetCommandLineArgs();
            if (argsv.Length > 1)
                foreach (string x in argsv)
                    switch (x)
                    {
                        case "autoconnect":
                            Button_Click(null, null);
                            break;
                        case "traystart":
                            Hide();
                            Tray = true;
                            break;
                    }

            //WebSocket сервер визуалки
            WebSocketServer = new WebSocketSharp.Server.WebSocketServer("ws://localhost:8181");
            WebSocketServer.AddWebSocketService<WebSockServ>("/alert");
            WebSocketServer.Start();

            //Подключениее к OBS WebSocket
            if (MySave.Current.Bools[7])
            {
                OBSRstatus.Visibility = Visibility.Hidden;
                new Task(() =>
                {
                    OBSRemote = new OBSWebSock();
                }).Start();
            }

            //Подготовка TrueTTS
            lock (Extentions.SpeechSynth)
            {
                string Text = "А";
                Extentions.GetTrueTTSReady(Text, MySave.Current.YPV.ToString());
                Extentions.TrueTTS(Text);
                LastTTS = DateTime.Now;
            }
        }
        WebSocketSharp.Server.WebSocketServer WebSocketServer;
        WebServer WebServer;
        
        public static TwitchClient Client;
        TwitchAccount Account;
        private void ClientSendMessage(string text)
        {
            Client.SendMessage("‌" + text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.Content = "Подключение...";
            ConnectButton.IsEnabled = false;
            Streamer.IsEnabled = false;
            string[] lfstregme = Streamer.Text.Split('/');
            string strmer = lfstregme.LastOrDefault();
            if(string.IsNullOrEmpty(strmer))
                strmer = lfstregme[lfstregme.Length-2];
            MySave.Current.Streamer = strmer;
            new Task(() =>
            {
                while (Account == null)
                    Thread.Sleep(100);
                new Task(() =>
                {
                    while (string.IsNullOrEmpty(Account.Scopes))
                        Thread.Sleep(100);
                    if (!Account.CheckScopes("chat:edit", "chat:read"))
                    {
                        Extentions.AsyncWorker(() =>
                        {
                            if (File.Exists("account.txt"))
                                File.Delete("account.txt");
                            Process.Start(Extentions.AppFile);
                            Application.Current.Shutdown();
                        });
                    }
                    else if (Account.CheckScopes("channel:moderate"))
                    {
                        Extentions.AsyncWorker(() =>
                        {
                            GetModBtt.IsEnabled = false;
                        });
                    }
                }).Start();
                Client = new TwitchClient(Account, MySave.Current.Streamer, "", "", true);
                Client.OnMessage += Message;
                Client.OnReward += Reward;
                Client.OnBan += Ban;
                Client.Connect();
                //Console.WriteLine(Client.GetStreamerID());
                Rand = new Random(Rand.Next());
                Extentions.AsyncWorker(() =>
                {
                    MaxWidth = 1000;
                    MinWidth = 900;
                    MaxHeight = 600;
                    MinHeight = 500;
                    Width = 950;
                    Height = 550;
                    Top = Screen.PrimaryScreen.Bounds.Height/2 - ActualHeight / 2;
                    Left = Screen.PrimaryScreen.Bounds.Width / 2 - ActualWidth / 2;
                    ConnectButton.Visibility = Visibility.Collapsed;
                    Streamer.Visibility = Visibility.Collapsed;
                    Thanks.Visibility = Visibility.Visible;
                    Controls.Visibility = Visibility.Visible;
                    if(OBSRstatus.Visibility == Visibility.Hidden)
                        OBSRstatus.Visibility = Visibility.Visible;
                    if (Tray) Hide();
                    Status.Text = $"Подключено к: {Client.Streamer}(ID={Client.StreamerID})\n" +
                    $"Подключен как: {Client.Account.Login}(ID={Client.Account.UserID})\n" +
                    (GetModBtt.IsEnabled?"Модерация запрещена\n": "Модерация разрешена\n") +
                    $"Токен имеет права:\n{Client.Account.Scopes}\n";
                    //ConnectButton.Content = "Подключено";
                });
            }).Start();
        }

        Dictionary<string, string> Queue = new Dictionary<string, string>();
        (string, string) Current;
        private void Donation(object Sender, DonationEventArgs e)
        {
            if(MySave.Current.Bools[9])
            switch (e.MessageType)
            {
                case "text":
                        new Thread(() =>
                        {
                            if (MySave.Current.Bools[6])
                            {
                                e.Message = MyCensor.CensoreIT(e.Message);
                                e.NickName = MyCensor.CensoreIT(e.NickName);
                            }
                            lock (Extentions.SpeechSynth)
                            {
                                SpeechTask = Thread.CurrentThread;
                                int.TryParse(e.Amount, out int amnt);
                                string Text = (MySave.Current.Bools[11] ? $"{e.NickName} задонатил " : "") + (MySave.Current.Bools[12] ? $"{MoneyText(amnt, e.Currency)} со словами " : (MySave.Current.Bools[11] ? $" со словами " : "")) + e.Message;
                                Extentions.GetTrueTTSReady(Text, MySave.Current.DYPV.ToString());
                                if(MySave.Current.Nums[6] > 0)
                                {
                                    Thread.Sleep(MySave.Current.Nums[6]*1000);
                                }
                                if (MySave.Current.Bools[10] && File.Exists(MySave.Current.DTTSNTFL))
                                {
                                    Extentions.AsyncWorker(() =>
                                    {
                                        Extentions.Player.Open(new Uri(MySave.Current.DTTSNTFL, UriKind.Absolute));
                                        Extentions.Player.Play();

                                    });
                                    Thread.Sleep(1000);
                                    Thread.Sleep(MediaDurationMs);
                                }
                                Extentions.TrueTTS(Text);
                                Thread.Sleep(100);
                                while (Extentions.SpeechSynth.State == SynthesizerState.Speaking)
                                {
                                    Thread.Sleep(100);
                                }
                            }
                        }).Start();
                    break;
                case "sound":
                    
                    break;
            }
        }

        string MoneyText(int number, string concu)
        {
            if (((number % 100) > 10) && ((number % 100) < 20))
            {
                switch (concu)
                {
                    case "RUB": return number+"рублей";
                    default: return number+"едениц денег";
                }
            }
            if (number % 10 == 1)
            {
                switch (concu)
                {
                    case "RUB": return number + "рубль";
                    default: return number + "еденицу денег";
                }
            }
            if ((number % 10 == 2) || (number % 10 == 3) || (number % 10 == 4))
            {
                switch (concu)
                {
                    case "RUB": return number + "рубля";
                    default: return number + "еденицы денег";
                }
            }
            switch (concu)
            {
                case "RUB": return number + "рублей";
                default: return number + "едениц денег";
            }
        }
        private void Ban(object Sender, BanEventArgs e)
        {
            switch (e.Type)
            {
                case BanType.BanOrTimeout:
                    string Nick = e.NickName.ToLower();
                    if (Current.Item2 == Nick)
                        Extentions.AsyncWorker(() => AcSwitch(null));
                    foreach (var item in Queue.Where(kvp => kvp.Value == Nick).ToList())
                    {
                        Queue.Remove(item.Key);
                    }
                    break;
                case BanType.MsgDelete:
                    if (Current.Item1 == e.MessageID)
                        Extentions.AsyncWorker(() => AcSwitch(null));
                    if (Queue.ContainsKey(e.MessageID))
                        Queue.Remove(e.MessageID);
                    break;
            }
        }
        private void Reward(object Sender, RewardEventArgs e)
        {
            //Console.WriteLine(e.CustomRewardID + "|" + e.Title);
            if (!string.IsNullOrEmpty(e.CustomRewardID))
            {
                RewardTrapHatch?.Invoke(this, e);
                new Thread(() =>
                {
                    if (MySave.Current.Bools[6])
                        e.Text = MyCensor.CensoreIT(e.Text);
                    RewEvents.FirstOrDefault(x => x.CustomRewardID == e.CustomRewardID)?.invoke(e);
                }).Start();
            }
        }

        Random Rand = new Random();
        bool IsVoting;
        DateTime LastTTS;

        bool IgnoreMessages = false;
        private void Message(object Sender, MessageEventArgs e)
        {
            string lowNick = e.NickName.ToLower().Trim();
            if (IgnoreMessages && !e.Message.StartsWith(">enable")) return;
            if (lowNick == Client.Account.Login && e.Message.Contains("‌")) return;
            if (MySave.Current.Bools[6])
                e.Message = MyCensor.CensoreIT(e.Message);
            
            UserRights permlvl = 0;
            if (lowNick == Client.Streamer)
                permlvl = UserRights.All;
            else
            {
                if(lowNick == "scriptedengineer")
                    permlvl |= UserRights.Создатель;
                if (e.Flags.HasFlag(ExMsgFlag.FromModer))
                    permlvl |= UserRights.Модератор;
                if (e.Flags.HasFlag(ExMsgFlag.FromVip))
                    permlvl |= UserRights.VIP;
                if (MySave.UsersRights.ContainsKey(lowNick))
                    permlvl |= MySave.UsersRights[lowNick];
                if (MySave.TmpUsersRights.ContainsKey(lowNick))
                    permlvl |= MySave.TmpUsersRights[lowNick];
            }
            //if (lowNick == Client.Account.Login)return;
            if (IsVoting)
                IfVoteAdd(e);
            try
            {
                string[] taste = e.Message.Split(new char[] { '>' },2);
                if (taste.Length == 2)
                {
                    string[] args = taste[1].Trim('\r', '\n').Split(new char[] { ' ' });
                    string cmd = args.First().ToLower();
                    switch (cmd)
                    {
                        case "ping":
                            if (permlvl.HasFlag(UserRights.ping) || permlvl.HasFlag(UserRights.Создатель))
                                ClientSendMessage(e.NickName + ", pong");
                            break;
                        case "help":
                        case "rights":
                            if (permlvl == UserRights.All && args.Length > 1)
                            {
                                UserRights usrddf = UserRights.Зритель;
                                if (MySave.UsersRights.ContainsKey(args[1].ToLower()))
                                    usrddf |= MySave.UsersRights[args[1].ToLower()];
                                if (MySave.TmpUsersRights.ContainsKey(args[1].ToLower()))
                                    usrddf |= MySave.TmpUsersRights[args[1].ToLower()];
                                if (usrddf != UserRights.Зритель)
                                    ClientSendMessage("Для " + args[1].ToLower() + ", дополнительно, доступны следующие команды:"
                                    + (usrddf.HasFlag(UserRights.ping) ? " >ping" : "")
                                    + (usrddf.HasFlag(UserRights.speech) ? " >speech [Text]" : "")
                                    + (usrddf.HasFlag(UserRights.tts) ? " >tts [Text]" : "")
                                    + (usrddf.HasFlag(UserRights.notify) ? " >notify" : "")
                                    + (usrddf.HasFlag(UserRights.coin) ? " >coin" : ""));
                                else
                                    ClientSendMessage("Для " + args[1].ToLower() + ", Доступны только команды соответствующие его статусу!");
                            }
                            else
                            {
                                if (permlvl != UserRights.Зритель)
                                    ClientSendMessage("Для " + e.NickName.ToLower() + " доступны следующие команды: >help"
                                    + (permlvl.HasFlag(UserRights.ping) ? " >ping" : "")
                                    + (permlvl.HasFlag(UserRights.speech) ? " >speech [Text]" : "")
                                    + (permlvl.HasFlag(UserRights.tts) ? " >tts [Text]" : "")
                                    + (permlvl.HasFlag(UserRights.notify) ? " >notify" : "")
                                    + (permlvl.HasFlag(UserRights.coin) ? " >coin" : "")
                                    + (permlvl.HasFlag(UserRights.Создатель) ? " >version >update" : "")
                                    + (permlvl.HasFlag(UserRights.Модератор) ? " >disable >enable >roullete [Time] [Count] >voting.start [Time] [Vote1]...[VoteN] >voting.result >voting.end" : "")
                                    + (permlvl.HasFlag(UserRights.All) ? " >rights.add [UserName] [Right] >rights.del [UserName] [Right]  >tmprights.add [UserName] [Right] >tmprights.del [UserName] [Right] >tts.cooldown [Seconds]" : ""));
                            }
                            break;
                        case "rights.add":
                            if (permlvl == UserRights.All && args.Length > 2)
                            {
                                if (!MySave.UsersRights.ContainsKey(args[1].ToLower()))
                                    MySave.UsersRights.Add(args[1].ToLower(), (UserRights)Enum.Parse(typeof(UserRights), args[2]));
                                else
                                    MySave.UsersRights[args[1].ToLower()] |= (UserRights)Enum.Parse(typeof(UserRights), args[2]);
                                
                            }
                            break;
                        case "rights.del":
                            if (permlvl == UserRights.All && args.Length > 2)
                            {
                                if (!MySave.UsersRights.ContainsKey(args[1].ToLower()))
                                    MySave.UsersRights.Add(args[1].ToLower(), UserRights.Зритель);
                                else
                                    MySave.UsersRights[args[1].ToLower()] &= ~(UserRights)Enum.Parse(typeof(UserRights), args[2]);
                            }
                            break;
                        case "tmprights.add":
                            if (permlvl == UserRights.All && args.Length > 2)
                            {
                                if (!MySave.TmpUsersRights.ContainsKey(args[1].ToLower()))
                                    MySave.TmpUsersRights.Add(args[1].ToLower(), (UserRights)Enum.Parse(typeof(UserRights), args[2]));
                                else
                                    MySave.TmpUsersRights[args[1].ToLower()] |= (UserRights)Enum.Parse(typeof(UserRights), args[2]);

                            }
                            break;
                        case "tmprights.del":
                            if (permlvl == UserRights.All && args.Length > 2)
                            {
                                if (!MySave.TmpUsersRights.ContainsKey(args[1].ToLower()))
                                    MySave.TmpUsersRights.Add(args[1].ToLower(), UserRights.Зритель);
                                else
                                    MySave.TmpUsersRights[args[1].ToLower()] &= ~(UserRights)Enum.Parse(typeof(UserRights), args[2]);
                            }
                            break;
                        case "voting.start":
                            if (permlvl.HasFlag(UserRights.Модератор) && args.Length > 3)
                            {
                                IsVoting = true;
                                Extentions.AsyncWorker(() =>
                                {
                                    if (int.TryParse(args[1], out int Minutes))
                                    {
                                        VotingSelect.SelectedIndex = 0;
                                        string[] Arrayz = new string[args.Length - 2];
                                        Array.Copy(args, 2, Arrayz, 0, args.Length - 2);
                                        SetVotes(Arrayz);
                                        StartVoting(Minutes);
                                        aTimer = new System.Timers.Timer(Minutes * 60000);
                                        bTimer = new System.Timers.Timer(Minutes * 15000);
                                        aTimer.Elapsed += EndVoting;
                                        bTimer.Elapsed += SendVotes;
                                        aTimer.Start();
                                        bTimer.Start();
                                    }
                                });
                            }
                            break;
                        case "voting.result":
                            if (permlvl.HasFlag(UserRights.Модератор))
                            {
                                SendVotes(null, null);
                            }
                            break;
                        case "voting.end":
                            if (permlvl.HasFlag(UserRights.Модератор))
                            {
                                EndVoting(null, null);
                            }
                            break;
                        case "version":
                            if (permlvl.HasFlag(UserRights.Создатель))
                            {
                                ClientSendMessage(e.NickName + ", " + Extentions.Version);
                            }
                            break;
                        case "update":
                            if (permlvl.HasFlag(UserRights.Создатель))
                            {
                                new Task(() =>
                                {
                                    string[] Vers = Extentions.ApiServer(ApiServerAct.CheckVersion).Split(' ');
                                    if ((Vers.Length == 3 && Vers[0] == "0") || (args.Length > 1 && args[1] == "rewrite"))
                                    {
                                        Extentions.SpeechSynth.SpeakAsyncCancelAll();
                                        Extentions.SpeechSynth.Rate = TTSrate;
                                        if (!File.Exists("udpateprotocol"))
                                            File.Create("udpateprotocol").Close();
                                        ClientSendMessage(e.NickName + ", обновляюсь!");
                                        Extentions.AsyncWorker(() =>
                                        {
                                            //TTSpeech.IsChecked = false;
                                            Window_Closed(null, null);
                                            File.WriteAllText("udpateprotocol", Tray.ToString());
                                            new Updater(Vers[1]).Show();

                                            Close();
                                        });
                                    }
                                    else
                                        ClientSendMessage(e.NickName + ", обновления не найдены!");
                                }).Start();
                            }
                            break;
                        case "disable":
                            if (permlvl.HasFlag(UserRights.Модератор))
                            {
                                ClientSendMessage(e.NickName + ", включено игнорирование чата!");
                                IgnoreMessages = true;
                            }
                            break;
                        case "enable":
                            if (permlvl.HasFlag(UserRights.Модератор))
                            {
                                ClientSendMessage(e.NickName + ", игнорирование чата отключено!");
                                IgnoreMessages = false;
                            }
                            break;
                        case "roullete":
                            if (permlvl.HasFlag(UserRights.Модератор) && args.Length > 2)
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    MinutesBox.Text = args[1];
                                    WinCount.Text = args[2];
                                    int Minutes = int.Parse(MinutesBox.Text);
                                    StartGame(Minutes);
                                    aTimer = new System.Timers.Timer(Minutes * 60000);
                                    aTimer.Elapsed += EndVoting;
                                    aTimer.Start();
                                });
                            }
                            break;
                        case "tts.cooldown":
                            if (permlvl.HasFlag(UserRights.All))
                            {
                                if (args.Length > 1)
                                {
                                    int.TryParse(args[1],out int x);
                                    MySave.Current.Nums[5] = x;
                                }
                            }
                            break;
                        case "speech":
                            if (permlvl.HasFlag(UserRights.tts))
                            {
                                if (args.Length > 1 && (DateTime.Now-LastTTS).TotalSeconds > MySave.Current.Nums[5])
                                {
                                    lock (Extentions.SpeechSynth)
                                    {
                                        string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                                        Extentions.TextToSpeech(Text);
                                        LastTTS = DateTime.Now;
                                        while (Extentions.SpeechSynth.State == SynthesizerState.Speaking)
                                        {
                                            Thread.Sleep(100);
                                        }
                                    }
                                }
                            }
                            break;
                        case "tts":
                            if (permlvl.HasFlag(UserRights.tts))
                            {
                                if (args.Length > 1 && (DateTime.Now - LastTTS).TotalSeconds > MySave.Current.Nums[5])
                                {
                                    lock (Extentions.SpeechSynth)
                                    {
                                        string Text = taste[1].Split(new char[] { ' ' }, 2).Last();
                                        Extentions.GetTrueTTSReady(Text, MySave.Current.YPV.ToString());
                                        Extentions.TrueTTS(Text);
                                        LastTTS = DateTime.Now;
                                    }
                                }
                            }
                            break;
                        case "notify":
                            if (permlvl.HasFlag(UserRights.notify))
                            {
                                Extentions.AsyncWorker(() =>
                                {
                                    Extentions.Player.Open(new Uri(MySave.Current.TTSNTFL, UriKind.Absolute));
                                    Extentions.Player.Play();
                                });
                            }
                            break;
                        case "coin":
                            if (permlvl.HasFlag(UserRights.coin))
                            {
                                string xo = "";
                                int monet = Rand.Next(0, 100);
                                if (monet % 2 == 0)
                                {
                                    xo = "Выпал орел.";
                                }
                                else if (monet > 90)
                                {
                                    xo = "Монетка встала на ребро.";
                                }
                                else
                                {
                                    xo = "Выпала решка.";
                                }
                                ClientSendMessage(e.NickName + ", " + xo);
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
                ClientSendMessage(e.NickName + ", было вызвано исключение во время обработки.");
            }
            //Console.WriteLine(e.CustomRewardID);
        }
        int TTSrate;
        private void Speech(MessageEventArgs e)
        {
            if (!MySave.Current.Bools[0])
                return;
            bool highlight = e.Flags.HasFlag(ExMsgFlag.Highlighted);
            bool speech = MySave.Current.Bools[0];
            switch (MySave.Current.Nums[1])
            {
                case 1:
                    speech &= highlight;
                    break;
                case 2:
                    speech &= e.CustomRewardID == MySave.Current.TTSCRID;
                    break;
            }
            if (speech)
            {
                bool TTSNotify = MySave.Current.Bools[2];
                new Task(() =>
                {
                    Queue.Add(e.ID, e.NickName.ToLower());
                    lock (Extentions.SpeechSynth)
                    {
                        if (!Queue.ContainsKey(e.ID))
                            return;
                        Queue.Remove(e.ID);
                        Current = (e.ID, e.NickName.ToLower());
                        if (!MySave.Current.Bools[0] || (e.Message.Length >= MySave.Current.Nums[3] && MySave.Current.Bools[4]))
                            return;
                        SpeechTask = Thread.CurrentThread;
                        TTSrate = Extentions.SpeechSynth.Rate;
                        if (e.Message.Length >= MySave.Current.Nums[3] && !MySave.Current.Bools[4])
                            Extentions.SpeechSynth.Rate = 10;
                        string Text = MySave.Current.Bools[3] ? $"{e.NickName} написал {e.Message}" : e.Message;
                        if (MySave.Current.Bools[8])
                            Extentions.GetTrueTTSReady(Text, MySave.Current.YPV.ToString());
                        if (TTSNotify && File.Exists(MySave.Current.TTSNTFL))
                        {
                            Extentions.AsyncWorker(() =>
                            {
                                if (!MySave.Current.Bools[0])
                                {
                                    return;
                                }
                                Extentions.Player.Open(new Uri(MySave.Current.TTSNTFL, UriKind.Absolute));
                                Extentions.Player.Play();

                            });
                            WebSockServ.SendAll("Alert", string.Format("{0}|{1}", e.NickName, e.Message));
                            Thread.Sleep(1000);
                            Thread.Sleep(MediaDurationMs);
                        }
                        else
                        {
                            WebSockServ.SendAll("Alert", string.Format("{0}|{1}", e.NickName, e.Message));
                            Thread.Sleep(1000);
                        }
                        
                        Extentions.AsyncWorker(() =>
                        {
                            if (!MySave.Current.Bools[0])
                            {
                                WebSockServ.SendAll("Close");
                                return;
                            }
                            if (!MySave.Current.Bools[8])
                                Extentions.TextToSpeech(Text);
                        });
                        if(MySave.Current.Bools[8])
                            Extentions.TrueTTS(Text);
                        Thread.Sleep(100);
                        while (Extentions.SpeechSynth.State == SynthesizerState.Speaking)
                        {
                            Thread.Sleep(100);
                        }
                        WebSockServ.SendAll("Close");
                        Extentions.SpeechSynth.Rate = TTSrate;
                    }
                }).Start();
            }
        }
        int VoteMax = 1;
        Dictionary<string, int> Votings = new Dictionary<string, int>();
        private void IfVoteAdd(MessageEventArgs e)
        {
            string msg = e.Message.Trim();
            bool isvote = int.TryParse(msg, out int vote);
            if (!isvote && Votes.ContainsValue(msg))
            {
                vote = Votes.FirstOrDefault(x => x.Value == msg).Key;
                if (vote != 0) isvote = true;
            }
            if (isvote && vote <= VoteMax && vote >= 1)
            {
                if (Votings.ContainsKey(e.NickName))
                {
                    Votings[e.NickName] = vote;
                }
                else
                {
                    Votings.Add(e.NickName, vote);
                    Extentions.AsyncWorker(() =>
                    {
                        UserList.Items.Add(new Voter(e.NickName, Votes[vote]));
                    });
                }
            }
            if (!IsGame)
                DisplayVotes();
            Extentions.AsyncWorker(() =>
            {
                foreach (Voter X in UserList.Items)
                {
                    if (X.Nickname == e.NickName && Votes.ContainsKey(vote))
                        X.Vote = Votes[vote];
                }
                UserList.Items.SortDescriptions.Clear();
                UserList.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("ID", System.ComponentModel.ListSortDirection.Ascending));
            });
        }
        Dictionary<int, string> Votes = new Dictionary<int, string>();
        private (string, string) GetVotes(bool addVotes = false)
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
            
            for(int index = 1; index <= Votes.Count; index++)//ListElement X in VotingList.Items
            {
                
                var kvpe = 0;
                if (voting.ContainsKey(index))
                    kvpe = voting[index];
                end += " " + (addVotes? "":index + "-") + Votes[index] + " [" + (kvpe == 0?0:(float)kvpe / (float)Votings.Count()).ToString("0.0%") + "]; ";
            }
            //string kvpo = "";
            string win = "";
            if (addVotes && Winner != -1)
                win = "Победил: " + (Votes.ContainsKey(Winner) ? Votes[Winner] : Winner.ToString()) + "!";
            /*foreach (var kvpe in voting)
            {
                kvpo += "[" + (Votes.ContainsKey(kvpe.Key) ? Votes[kvpe.Key] : kvpe.Key.ToString()) + " = " + ((float)kvpe.Value / (float)Votings.Count()).ToString("0.0%") + "];   ";
            }*/
            return (win, (string.IsNullOrEmpty(end) ? "" : end) + " Проголосовало: " + Votings.Count);
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
                    if (Votings.Count() > 0)
                        percent = ((float)kvpe / (float)Votings.Count());
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
            if (IsVoting)
            {
                IsVoting = false;
                Extentions.AsyncWorker(() =>
                {
                    aTimer?.Close();
                    bTimer?.Close();
                    var x = GetVotes(true);
                    if (!IsGame)
                        ClientSendMessage($"Голосование окончено. {x.Item1} Результаты: {x.Item2}");
                    else
                    {
                        int countsdvin = 1;
                        int.TryParse(WinCount.Text,out countsdvin);
                        string vinners = "";
                        int[] sdx = Extentions.TrueRandom(0, Votings.Keys.Count-1, countsdvin);
                        foreach (int vinner in sdx)
                        {
                            if(vinner >= 0 && vinner < Votings.Keys.Count)
                                vinners += " "+Votings.Keys.ToArray()[vinner];
                            else
                                vinners += " [Ошибка]";
                        }
                        ClientSendMessage($"Рандом выбрал {(countsdvin == 1? "следующего зрителя":"следующих зрителей")}:"+ vinners);
                        IsGame = false;
                    }
                });
            }
            else
            {
                if (!IsGame)
                    ClientSendMessage("Голосование не ведется.");
            }
        }
        private void SendVotes(object sender, ElapsedEventArgs e)
        {
            if (IsVoting)
            {
                if (Votings.Count > 0)
                    ClientSendMessage("Голосование на текуший момент: " + GetVotes().Item2);
                else
                    ClientSendMessage("Еще никто не проголосавал.");
            }
            else if(sender == null)
            {
                ClientSendMessage("Голосование не ведется.");
            }
        }
        private void SetVotes(string[] votes)
        {
            VotingList.Items.Clear();
            int index = 0;
            foreach (string x in votes)
            {
                index++;
                ListElement item = new ListElement(VotingList.Items.Count, 2, 1);
                item.Strings[0] = x.Trim('\r', '\n', ' ');
                item.Strings[1] = "0.0%";
                item.Nums[0] = 0;
                VotingList.Items.Add(item);
            }
        }
        private void LoadVotes(string[] Exceps)
        {
            string filename = "./votings/Текущий.txt";
            if (VotingSelect.SelectedItem != null)
                filename = "./votings/" + VotingSelect.SelectedItem + ".txt";
            if (!File.Exists(filename))
                return;
            string data = File.ReadAllText(filename);
            string[] votes = data.Split('\n');
            VotingList.Items.Clear();
            int index = 0;
            bool useExcps = Exceps.Length == 0;
            foreach (string x in votes)
            {
                index++;
                if (useExcps || Exceps.Contains(index.ToString()))
                {
                    ListElement item = new ListElement(VotingList.Items.Count, 2, 1);
                    item.Strings[0] = x.Trim('\r', '\n', ' ');
                    item.Strings[1] = "0.0%";
                    item.Nums[0] = 0;
                    VotingList.Items.Add(item);
                }
            }
        }
        private void SaveVotes(string ToFile = "./votings/new.txt")
        {
            StringBuilder votesave = new StringBuilder();
            bool xolof = false;
            foreach (ListElement x in VotingList.Items)
            {
                if(xolof)
                    votesave.Append("\n");
                votesave.Append(x.Strings[0]);
                xolof = true;
            }
            File.WriteAllText(ToFile, votesave.ToString());
        }
        private void StartVoting(int Minutes = 0)
        {
            Votings.Clear();
            UserList.Items.Clear();
            //DisplayVotes();
            string Vrotes = ""; int index = 0;
            foreach (ListElement X in VotingList.Items)
            {
                index++;
                Vrotes += "  " + index + "-" + X.Strings[0] + ";";
            }
            IsVoting = true;
            //string eXtraString = "";//(" " + Rand.Next(-100, 100).ToString());
            VoteMax = VotingList.Items.Count;
            ClientSendMessage("Голосование запущено, напишите цифру от 1 до " + VoteMax + " в чат чтобы проголосовать. " + Vrotes + (Minutes != 0?$" У вас {Minutes} {Mins(Minutes)}.":""));
            index = 0;
            Votes.Clear();
            foreach (ListElement X in VotingList.Items)
            {
                index++;
                //Votes += "  (" + index + "-" + X.Strings[0] + ");";
                Votes.Add(index, X.Strings[0]);
            }
        }
        bool IsGame = false;
        private void StartGame(int Minutes = 0)
        {
            Votings.Clear();
            UserList.Items.Clear();
            IsVoting = true;
            IsGame = true;
            Votes.Clear();
            int.TryParse(WinCount.Text, out int countsdvin);
            ClientSendMessage($"Через {Minutes} {Mins(Minutes,true)} {SelUS(countsdvin)}, напиши 'play' или '1' что-бы принять участие!");
            Votes.Add(1, "play");

        }
        string SelUS(int number)
        {
            if (((number % 100) > 10) && ((number % 100) < 20))
                return $"будут выбраны {number} случайных пользователей";
            if (number % 10 == 1)
                return $"будет выбран {number} случайный пользователь";
            if ((number % 10 == 2) || (number % 10 == 3) || (number % 10 == 4))
                return $"будут выбраны {number} случайных пользователя";
            return $"будут выбраны {number} случайных пользователей";
        }
        string Mins(int number, bool lt = false)
        {
            if (((number % 100) > 10) && ((number % 100) < 20))
                return "минут";
            if (number % 10 == 1)
                return lt ? "минуту" : "минута";
            if ((number % 10 == 2) || (number % 10 == 3) || (number % 10 == 4))
                return "минуты";
            return "минут";
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
            ListElement Add = new ListElement(VotingList.Items.Count, 2, 1);
            Add.Strings[0] = "Новый";
            Add.Strings[1] = "0.0%";
            Add.Nums[0] = 0;
            VotingList.Items.Add(Add);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int Minutes = int.Parse(MinutesBox.Text);
            StartVoting(Minutes);
            aTimer = new System.Timers.Timer(Minutes * 60000);
            bTimer = new System.Timers.Timer(Minutes * 15010);
            aTimer.Elapsed += EndVoting;
            bTimer.Elapsed += SendVotes;
            aTimer.Start();
            bTimer.Start();
            ((Button)sender).IsEnabled = false;
            new Thread(() => {
                Thread.Sleep(1000);
                Extentions.AsyncWorker(() => { 
                    ((Button)sender).IsEnabled = true;
                });
            }).Start();
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            EndVoting(null, null);
            ((Button)sender).IsEnabled = false;
            new Thread(() => {
                Thread.Sleep(1000);
                Extentions.AsyncWorker(() => {
                    ((Button)sender).IsEnabled = true;
                });
            }).Start();
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (VotingList.SelectedIndex != -1)
                VotingList.Items.RemoveAt(VotingList.SelectedIndex);
            else if (VotingList.Items.Count > 0)
                VotingList.Items.RemoveAt(VotingList.Items.Count - 1);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.SaveFileDialog dial = new System.Windows.Forms.SaveFileDialog())
            {
                dial.InitialDirectory = System.IO.Path.GetDirectoryName(Extentions.AppFile)+"\\votings";
                dial.Filter = "txt files (*.txt)|*.txt";
                if(dial.ShowDialog() == System.Windows.Forms.DialogResult.OK){
                    SaveVotes(dial.FileName);
                }
            } 
        }

        byte lastselected = 255;
        private void VotingSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VotingSelect.SelectedIndex != -1 && lastselected != 255)
            {
                SaveVotes("./votings/" + VotingSelect.Items[lastselected] + ".txt");
                LoadVotes(new string[0]);
                lastselected = (byte)VotingSelect.SelectedIndex;
            }
            else if(lastselected == 255)
            {
                lastselected = 0;
            }
        }
        private void TTSpeech_Checked(object sender, RoutedEventArgs e)
        {
            if (!TTSpeech.IsChecked.Value)
            {
                Extentions.Player.Stop();
                Extentions.SpeechSynth.SpeakAsyncCancelAll();
                SpeechTask?.Abort();
                Extentions.SpeechSynth.Rate = TTSrate;
            }
            MySave.Current.Bools[0] = TTSpeech.IsChecked.Value;
        }

        private void Voices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Extentions.SpeechSynth.SelectVoice(Voices.SelectedItem.ToString());
            //Extentions.SpeechSynth.SelectVoice("Microsoft Pavel");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                if (MySave.Current.Bools[5])
                {
                    Hide();
                    Tray = true;
                }
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int num = (int)Math.Max(Math.Min(SynthSpeed.Value, 10), -10);
            Extentions.SpeechSynth.Rate = num;
            TTSrate = num;
            SynthSpeed.Value = num;
            SpeedLabel.Content = $"Скорость ({num}) [{Extentions.RateToSpeed()}x]";
            MySave.Current.Nums[2] = num;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog Dial = new System.Windows.Forms.OpenFileDialog();
            Dial.Filter = "Аудиофайл(*.mp3)|*.mp3";
            if (Dial.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Dial.FileName;
                MySave.Current.TTSNTFL = file;
                TTSNotifyLabel.Content = System.IO.Path.GetFileName(file);
            }
        }
        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            int carret = Sender.CaretIndex;
            int.TryParse(Sender.Text, out int MaxTTS);
            Sender.Text = MaxTTS.ToString();
            Sender.CaretIndex = carret;
            MySave.Current.Nums[3] = MaxTTS;
        }


        private void AcSwitch(WinHotKey Key)
        {
            WebSockServ.SendAll("Close");
            Extentions.Player.Stop();
            Extentions.SpeechSynth.SpeakAsyncCancelAll();
            SpeechTask?.Abort();
            Extentions.SpeechSynth.Rate = TTSrate;
        }

        WinHotKey SwitcherKey;
        int keyMode = -1;
        private void TextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift
            || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl
            || e.Key == Key.LeftAlt || e.Key == Key.RightAlt) keyMode = -1;
            else
            {
                Key key = e.Key;
                if (key == Key.System)
                {
                    key = e.SystemKey;
                    keyMode = 0;
                }
                else if (keyMode == 0)
                    keyMode = -1;

                SwitcherKey?.Unregister();
                SwitcherKey?.Dispose();
                string mode = "";
                try
                {
                    switch (keyMode)
                    {
                        case 0:
                            mode = "Alt+";
                            SwitcherKey = new WinHotKey(key, KeyModifier.Alt, AcSwitch);
                            MySave.Current.HotkeyModifier = KeyModifier.Alt;
                            break;
                        case 1:
                            mode = "Ctrl+";
                            SwitcherKey = new WinHotKey(key, KeyModifier.Ctrl, AcSwitch);
                            MySave.Current.HotkeyModifier = KeyModifier.Ctrl;
                            break;
                        case 2:
                            mode = "Shift+";
                            SwitcherKey = new WinHotKey(key, KeyModifier.Shift, AcSwitch);
                            MySave.Current.HotkeyModifier = KeyModifier.Shift;
                            break;
                        default:
                            SwitcherKey = new WinHotKey(key, KeyModifier.None, AcSwitch);
                            MySave.Current.HotkeyModifier = KeyModifier.None;
                            break;
                    }
                    MySave.Current.Hotkey = key;
                    ((TextBox)sender).Text = (mode) + key;
                }
                catch
                {

                }
            }
        }
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) keyMode = 2;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) keyMode = 1;
        }

        EventHandler<RewardEventArgs> RewardTrapHatch;

        private void RewardTrap_Click(object sender, RoutedEventArgs e)
        {
            if (RewardTrap.Content.ToString() == "Отмена")
            {
                RewardTrap.Content = "Сканировать товар";
                RewardTrapHatch -= TTSRewardTrap;
            }
            else
            {
                RewardTrap.Content = "Отмена";
                RewardTrapHatch += TTSRewardTrap;
            }
        }
        private void TTSRewardTrap(object sender, RewardEventArgs e)
        {
            Extentions.AsyncWorker(() =>
            {
                CustomRewardID.Text = e.CustomRewardID;
                MySave.Current.TTSCRID = e.CustomRewardID;
                RewardTrap.Content = "Сканировать товар";
                RewardName.Text = e.Title;
                MySave.Current.TTSCRTitle = e.Title;
                RewardTrapHatch -= TTSRewardTrap;
            });
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            MySave.Current.Bools[0] = TTSpeech.IsChecked.Value;
            MySave.Current.Bools[1] = TTSpeechOH.IsChecked.Value;
            MySave.Current.Bools[2] = TTSNotifyUse.IsChecked.Value;
            MySave.Current.Bools[3] = TTSNicks.IsChecked.Value;
            MySave.Current.Bools[4] = DontTTS.IsChecked.Value;
            MySave.Current.Bools[5] = MinimizeToTray.IsChecked.Value;
            MySave.Current.Nums[0] = Voices.SelectedIndex;
            MySave.Current.Nums[1] = AllChat.IsChecked.Value ? 0 : (TTSpeechOH.IsChecked.Value ? 1 : (CustomReward.IsChecked.Value ? 2 : -1));
            MySave.Current.TTSCRID = CustomRewardID.Text;
            MySave.Save();
            SaveEvents();
            if (VotingSelect.SelectedIndex != -1)
                SaveVotes("./votings/" + VotingSelect.Items[lastselected] + ".txt");
            ni.Visible = false;
            //Application.Current.Shutdown();
        }
        private void SaveEvents()
        {
            RewardEvent[] Svrkghksdfjn = RewEvents.ToArray();
            XmlSerializer formatter = new XmlSerializer(typeof(RewardEvent[]));
            if (File.Exists("rewards.xml")) File.Delete("rewards.xml");
            using (FileStream fs = new FileStream("rewards.xml", FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, Svrkghksdfjn);
            }
        }
        private void LoadEvents()
        {
            if (File.Exists("rewards.xml"))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(RewardEvent[]));
                using (FileStream fs = new FileStream("rewards.xml", FileMode.OpenOrCreate))
                {
                    RewEvents = ((RewardEvent[])formatter.Deserialize(fs)).ToList();
                }
            }
            foreach(var x in RewEvents)
            {
                EvList.Items.Add(x.EventName);
            }
        }

        private void TTSNicks_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[3] = TTSNicks.IsChecked.Value;
        }
        private void AllChat_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MySave.Current.Nums[1] = AllChat.IsChecked.Value ? 0 : (TTSpeechOH.IsChecked.Value ? 1 : (CustomReward.IsChecked.Value ? 2 : -1));
            }
            catch
            {

            }
        }
        private void TTSpeechOH_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if(AllChat != null)
                    MySave.Current.Nums[1] = AllChat.IsChecked.Value ? 0 : (TTSpeechOH.IsChecked.Value ? 1 : (CustomReward.IsChecked.Value ? 2 : -1));
            }
            catch
            {

            }
        }
        private void CustomReward_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MySave.Current.Nums[1] = AllChat.IsChecked.Value ? 0 : (TTSpeechOH.IsChecked.Value ? 1 : (CustomReward.IsChecked.Value ? 2 : -1));
            }
            catch
            {

            }
        }
        private void CustomRewardID_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySave.Current.TTSCRID = CustomRewardID.Text;
        }
        private void TTSNotifyUse_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[2] = TTSNotifyUse.IsChecked.Value;
        }
        private void DontTTS_Checked(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[4] = true;
        }
        private void SMaxTTS_Checked(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[4] = false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (MinimizeToTray.IsChecked == null)
                return;
            MySave.Current.Bools[5] = MinimizeToTray.IsChecked.Value;
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox X = (TextBox)sender;
            if (int.TryParse(X.Uid, out int ID) && VotingList.Items.Count > ID)
            {
                ListElement Y = (ListElement)VotingList.Items[ID];
                Y.Strings[0] = X.Text;
            }
        }


        List<RewardEvent> RewEvents = new List<RewardEvent>();
        private void EventRewardTrap_Click(object sender, RoutedEventArgs e)
        {
            if (EventRewardTrap.Content.ToString() == "Отмена")
            {
                EventRewardTrap.Content = "Сканировать товар";
                RewardTrapHatch -= EventRewardTrapt;
            }
            else
            {
                EventRewardTrap.Content = "Отмена";
                RewardTrapHatch += EventRewardTrapt;
            }
        }
        private void EventRewardTrapt(object sender, RewardEventArgs e)
        {
            Extentions.AsyncWorker(() =>
            {
                if (EvList.SelectedIndex == -1)
                    return;
                RewardEvent RewEv = RewEvents[EvList.SelectedIndex];
                RewEv.RewardName = e.Title;
                EventRewardName.Text = RewEv.RewardName;
                RewEv.CustomRewardID = e.CustomRewardID;
                CustomEventRewardID.Text = RewEv.CustomRewardID;
                RewEvents[EvList.SelectedIndex] = RewEv;
                EventRewardTrap.Content = "Сканировать товар";
                RewardTrapHatch -= EventRewardTrapt;
            });
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            RewEvents.Add(new RewardEvent());
            EvList.Items.Add("Новый");
        }
        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            if (EvList == null || EvList.SelectedIndex == -1)
                return;
            RewardEvent RewEv = RewEvents[EvList.SelectedIndex];
            RewEv.EventName = EvName.Text;
            int ind = EvList.SelectedIndex;
            EvList.Items[ind] = EvName.Text;
            EvList.SelectedIndex = ind;
        }
        private void EvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EvName == null || EvList == null || EvList.SelectedIndex == -1) 
            {
                CustomEventRewardID.IsEnabled = false;
                EventRewardTrap.IsEnabled = false;
                EvName.IsEnabled = false;
                EventRewardName.IsEnabled = false;
                Script.IsEnabled = false;
                return;
            }
            CustomEventRewardID.IsEnabled = true;
            EventRewardTrap.IsEnabled = true;
            EvName.IsEnabled = true;
            EventRewardName.IsEnabled = true;
            Script.IsEnabled = true;
            RewardEvent RewEv = RewEvents[EvList.SelectedIndex];
            CustomEventRewardID.Text = RewEv.CustomRewardID;
            EvName.Text = RewEv.EventName;
            EventRewardName.Text = RewEv.RewardName;
            Script.Text = RewEv.Script;
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (EvList == null || EvList.SelectedIndex == -1)
                return;
            RewEvents.RemoveAt(EvList.SelectedIndex);
            EvList.Items.RemoveAt(EvList.SelectedIndex);
            
        }

        private void Censor_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySave.Current.Censor = Censor.Text;
            MyCensor.Replacer = MySave.Current.Censor;
        }

        private void BadWords_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySave.Current.BadWords = BadWords.Text.ToLower();
            MyCensor.MyBadWords = MySave.Current.BadWords.Split(' ').ToList();
            MyCensor.MyBadWords.Sort((x, y) => x.Length > y.Length ? -1 : (x.Length == y.Length ? 0 : 1));
        }

        private void Filtred_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[6] = Filtred.IsChecked.Value;
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int num = (int)Math.Max(Math.Min(Volume.Value, 100), 0);
            Extentions.SpeechSynth.Volume = num;
            Extentions.Player.Volume = num/100d;
            Volume.Value = num;
            VolumeLabel.Content = $"Громкость ({num})";
            MySave.Current.Nums[4] = num;
        }

        private void OBSRemEn_Click(object sender, RoutedEventArgs e)
        {
            if (OBSRemEn.IsChecked == null)
                return;
            MySave.Current.Bools[7] = OBSRemEn.IsChecked.Value;
        }

        private void OBS_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            MySave.Current.OBSWSPort = OBS_port.Text;
        }

        private void OBSRmPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            MySave.Current.OBSWSPass = OBSRmPass.Password;
           
        }

        private void OBSRmPass_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                OBSWebSock.ReAuth();
            }
            catch
            {

            }
        }

        private void UseYA_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[8] = UseYA.IsChecked.Value;
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            if (File.Exists("account.txt")) 
                File.Delete("account.txt");
            Process.Start("https://id.twitch.tv/oauth2/authorize?response_type=token&client_id=v1wv59aw5a8w2reoyq1i5j6mwb1ixm&redirect_uri=http://localhost:8190/twitchcode&scope=chat:edit%20chat:read%20channel:moderate");
            while (!File.Exists("account.txt"))
            {
                Thread.Sleep(500);
            }
            Thread.Sleep(500);
            Process.Start(Extentions.AppFile);
            Application.Current.Shutdown();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Script_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EvList.SelectedIndex == -1)
                return;
            RewardEvent RewEv = RewEvents[EvList.SelectedIndex];
            RewEv.Script = Script.Text;
        }

        private void Voices_TTTS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MySave.Current.YPV = (YVoices)Voices_TTTS.SelectedIndex;
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            if(Script.IsEnabled)
                if (string.IsNullOrEmpty(Script.Text))
                {
                    Button Sender = (Button)sender;
                    Script.Text = Sender.ToolTip.ToString();
                }
                else
                {
                    MessageBox.Show("Пожалуйста сохраните алгоритм и очистите поле ввода алгоритма!");
                }
        }

        private void DAConnect_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("da.txt"))
                File.Delete("da.txt");
            Process.Start("https://www.donationalerts.com/oauth/authorize?client_id=865&redirect_uri=http://localhost:8190/da&response_type=code&scope=oauth-donation-subscribe+oauth-user-show");
            while (!File.Exists("da.txt"))
            {
                Thread.Sleep(500);
            }
            Thread.Sleep(500);
            Process.Start(Extentions.AppFile);
            Application.Current.Shutdown();
            //"https://www.donationalerts.com/oauth/authorize?client_id=865&redirect_uri=http://localhost:8190/da&response_type=code&scope=oauth-donation-subscribe"
        }

        private void DATTSEnable_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[9] = DATTSEnable.IsChecked.Value;
        }

        private void DANotify_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[10] = DANotify.IsChecked.Value;
        }

        private void DTTSNicks_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[11] = DTTSNicks.IsChecked.Value;
        }

        private void DTTSAmount_Click(object sender, RoutedEventArgs e)
        {
            MySave.Current.Bools[12] = DTTSAmount.IsChecked.Value;
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog Dial = new System.Windows.Forms.OpenFileDialog();
            Dial.Filter = "Аудиофайл(*.mp3,*.wav)|*.mp3;*.wav";
            if (Dial.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = Dial.FileName;
                MySave.Current.DTTSNTFL = file;
                TTSNotifyLabel_Copy.Content = System.IO.Path.GetFileName(file);
            }
            
        }

        private void Voices_TTTS_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MySave.Current.DYPV = (YVoices)Voices_TTTS_Copy.SelectedIndex;
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            int Minutes = int.Parse(MinutesBox.Text);
            StartGame(Minutes);
            aTimer = new System.Timers.Timer(Minutes * 60000);
            aTimer.Elapsed += EndVoting;
            aTimer.Start();
            ((Button)sender).IsEnabled = false;
            new Thread(() => {
                Thread.Sleep(1000);
                Extentions.AsyncWorker(() => {
                    ((Button)sender).IsEnabled = true;
                });
            }).Start();
        }

        private void WaitPlease_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            int carret = Sender.CaretIndex;
            int.TryParse(Sender.Text, out int MaxTTS);
            Sender.Text = MaxTTS.ToString();
            Sender.CaretIndex = carret;
            MySave.Current.Nums[6] = MaxTTS;
        }

        private void MinutesBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox Sender = (TextBox)sender;
            int carret = Sender.CaretIndex;
            int.TryParse(Sender.Text, out int MaxTTS);
            Sender.Text = MaxTTS.ToString();
            Sender.CaretIndex = carret;
        }

        private void CustomEventRewardID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EvList.SelectedIndex == -1)
                return;
            RewardEvent RewEv = RewEvents[EvList.SelectedIndex];
            RewEv.CustomRewardID = CustomEventRewardID.Text;
        }



    }
}
