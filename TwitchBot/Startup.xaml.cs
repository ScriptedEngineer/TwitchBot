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
            //Загрузка зависимостей программы
            if (!File.Exists("TwitchLib.dll") || !File.Exists("websocket-sharp.dll"))
            {
                new Updater().Show();
            }
            else
            {
                new MainWindow().Show();
            }
            InitializeComponent();
            Close();
        }
    }
}
