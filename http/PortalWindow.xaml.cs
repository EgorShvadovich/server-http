﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace http
{
    /// <summary>
    /// Логика взаимодействия для PortalWindow.xaml
    /// </summary>
    public partial class PortalWindow : Window
    {
        private String projectDir;

        public PortalWindow()
        {
            InitializeComponent();

            String currentDir = Directory.GetCurrentDirectory();
            // находим в пути "Http", до этого места путь будет общий для всех
            int httpPosition = currentDir.IndexOf("http");
            // берем часть строки до этой позиции
            projectDir = currentDir[..httpPosition];
        }

        private void ChatServer_Click(object sender, RoutedEventArgs e)
        {
            // задаем размещение ЕХЕ файла server.exe
            String serverPath = @"server\bin\Debug\net7.0-windows\server.exe";
            // Создаем процесс = запуск приложения
            Process serverProcess = Process.Start(projectDir + serverPath);

            // TODO: ограничить запуск сервера: если он уже запущен, то выдавать
            // вопрос "запускать ли еще один сервер"
        }

        private void ChatClient_Click(object sender, RoutedEventArgs e)
        {
            String serverPath = @"client\bin\Debug\net7.0-windows\client.exe";
            Process clientProcess = Process.Start(projectDir + serverPath);
        }

        private void HttpRequests_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
        }

        private void ApiRequests_Click(object sender, RoutedEventArgs e)
        {
            new ApiWindow().Show();
        }
        private void ApiCoin_Click(object sender, RoutedEventArgs e)
        {
            new ApiCoinWindow().Show();
        }

        private void SmtpButton_Click(object sender, RoutedEventArgs e)
        {
            new SmtpWindow().Show();
        }
    }
}
