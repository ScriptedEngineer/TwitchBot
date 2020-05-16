using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TwitchBot
{
    /// <summary>
    /// Логика взаимодействия для Startup.xaml
    /// </summary>
    public partial class Startup : Window
    {
        public Startup()
        {
            //Заметаем следы обновления
            if (File.Exists("update.vbs"))
            {
                File.Delete("update.vbs");
                while (File.Exists("TwitchLib.dll"))
                {
                    try
                    {
                        File.Delete("TwitchLib.dll");
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            //Загрузка зависимостей программы
            if (!File.Exists("TwitchLib.dll"))
            {
                try
                {
                    WebClient web = new WebClient();
                    web.DownloadFile(new Uri(@"https://wsxz.ru/downloads/TwitchLib.dll"), "TwitchLib.dll");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Ошибка загрузки TwitchLib.dll", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (!File.Exists("websocket-sharp.dll"))
            {
                try
                {
                    WebClient web = new WebClient();
                    web.DownloadFile(new Uri(@"https://wsxz.ru/downloads/websocket-sharp.dll"), "websocket-sharp.dll");
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Ошибка загрузки websocket-sharp.dll", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            new MainWindow().Show();
            InitializeComponent();
            Close();
        }
    }
}
