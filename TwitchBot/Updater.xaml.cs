using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Updater.xaml
    /// </summary>
    public partial class Updater : Window
    {
        readonly string UpdaterPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Updater.exe");
        public Updater()
        {
            InitializeComponent();
            string lastVersion, downloadURL;
            using (var client = new System.Net.WebClient())
            {
                client.Headers.Add("User-Agent", "SE-BlueprintEditor");
                client.Encoding = Encoding.UTF8;
                string git_ingo = client.DownloadString("https://api.github.com/repos/ScriptedEngineer/AutoUpdater/releases");
                lastVersion = Extentions.RegexMatch(git_ingo, @"""tag_name"":""([^""]*)""");
                downloadURL = Extentions.RegexMatch(git_ingo, @"""browser_download_url"":""([^""]*)""");
            }
            bool updaterNeedUpdate = true;
            if (File.Exists(UpdaterPath))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(UpdaterPath);
                updaterNeedUpdate = Extentions.CheckVersion(lastVersion, versionInfo.ProductVersion);
            }
            if (updaterNeedUpdate)
            {
                Status.Content = "Загрузка...";
                WebClient web = new WebClient();
                web.DownloadFileAsync(new Uri(downloadURL), UpdaterPath);
                web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
                web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            }
            else
            {
                DownloadFileCompleted(this, null);
            }
        }
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress.Value = e.ProgressPercentage;
            Procents.Content = e.ProgressPercentage + "%";
            Status.Content = "Скачиваем загрузчик... (" + (e.BytesReceived / 1024) + "kb из " + (e.TotalBytesToReceive / 1024) + "kb)";
        }
        public void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Process.Start(UpdaterPath, "/DarkTheme /WhatNew \"Что нового:\" /Available \"Доступна версия\" /Current \"Установленная версия\" /RemindLater \"Напомнить позже\" /UpdateNow \"Обновить сейчас\" /DownloadingFile \"Загрузка обновления\" /ExtractingUpdate \"Распаковка обновления\" /PleaseWait \"Пожалуйста подождите...\" /GitHub \"ScriptedEngineer/TwitchBot\" /RunApp \"" + Extentions.AppFile + "\"");
            Application.Current.Shutdown();
        }
    }
}
