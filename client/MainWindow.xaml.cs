using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.Json;
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

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] buffer = new byte[1024];
        private IPEndPoint? endpoint;  // копия - как у сервера
        private DateTime lastSyncMoment;
        private Random random;
        public MainWindow()
        {
            InitializeComponent();
            lastSyncMoment = DateTime.MinValue;
            random = new();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            authorTextBox.Text = "User " + random.Next(1, 100);
            ReCheckMessages();
        }

        private IPEndPoint? InitEndpoint()
        {
            if (endpoint is not null) return endpoint;
            try
            {
                IPAddress ip =               // На окне IP - это текст ("127.0.0.1")
                    IPAddress.Parse(         // Для его перевода в число используется
                        serverIp.Text);      // IPAddress.Parse
                int port =                   // Аналогично - порт
                    Convert.ToInt32(         // парсим число из текста
                        serverPort.Text);    // 
                endpoint =                   // endpoint - комбинация IP и порта
                    new IPEndPoint(ip, port);           // 
                return endpoint;
            }
            catch
            {
                MessageBox.Show("Check server network parameters");
                return null;
            }

        }
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if ((endpoint = InitEndpoint()) is null) return;
            if (string.IsNullOrWhiteSpace(messageTextBox.Text) || string.IsNullOrWhiteSpace(authorTextBox.Text))
            {
                MessageBox.Show("text or author is null");
                return;
            }
            ChatMessage chatMessage = new ChatMessage()
            {
                Author = authorTextBox.Text,
                Text = messageTextBox.Text,
                Moment = DateTime.Now
            };
            int wordsPerLine = 3;
            string[] words = chatMessage.Text.Split(" ");
            string wrappedText = "";
            int wordCount = 0;

            foreach (string word in words)
            {
                if (wordCount < wordsPerLine)
                {
                    wrappedText += word + " ";
                    wordCount++;
                }
                else
                {
                    wrappedText += Environment.NewLine + word + " ";
                    wordCount = 1;
                }
            }

            chatMessage.Text = wrappedText;
            messageTextBox.TextWrapping = TextWrapping.Wrap;
            SendMessage(chatMessage);
        }

        private void SendMessage(ChatMessage chatMessage)
        {
            if ((endpoint = InitEndpoint()) is null) return;
            if (string.IsNullOrWhiteSpace(messageTextBox.Text) || string.IsNullOrWhiteSpace(authorTextBox.Text))
            {
                MessageBox.Show("text or author is null");
                return;
            }
            Socket clientSocket = new Socket(        // создаем сокет подключения
                AddressFamily.InterNetwork,   // адресация IPv4
                SocketType.Stream,            // Двусторонний сокет (и читать, и писать)
                ProtocolType.Tcp);            // Протокол сокета - ТСР
            try
            {
                clientSocket.Connect(endpoint);
                // --------------------- соединение установлено ----------------------
                // сервер начинает с приема данных, поэтому клиент начинает с посылки
                // формируем объект-запрос
                ClientRequest request = new ClientRequest()
                {
                    Action = "Message",
                    Author = chatMessage.Author,
                    Text = chatMessage.Text,
                    Moment = chatMessage.Moment
                };
                // преобразуем объект в JSON
                SendRequest(clientSocket, request);

                var response = GetServerResponse(clientSocket);

                if (response is not null && response.Messages is not null)
                {
                    var message = response.Messages[0];
                    //chatLogs.Text += $"{message.Moment.ToShortTimeString()} {message.Author}: {message.Text}\n";
                    Label messageLabel = new()
                    {
                        Content = message.Text,
                        Background = Brushes.Salmon,
                        Margin = new Thickness(10, 5, 10, 5),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    chatContainer.Children.Add(messageLabel);
                }
                else
                {
                    chatLogs.Text += "ошибка доставки смс";
                }

                // после приема сервер отправляет подтверждение, клиент - получает
                MemoryStream stream = new MemoryStream();               // Другой способ получения
                do                                         // данных - собирать части
                {                                          // бинарного потока в 
                    int n = clientSocket.Receive(buffer);  // память.
                    stream.Write(buffer, 0, n);            // Затем создать строку
                } while (clientSocket.Available > 0);      // один раз пройдя
                String str = Encoding.UTF8.GetString(      // все полученные байты.
                    stream.ToArray());

                chatLogs.Text += str + "\n";

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
            }
            catch (Exception ex)
            {
                chatLogs.Text += ex.Message + "\n";
            }
        }

        private async void ReCheckMessages()
        {
            if ((endpoint = InitEndpoint()) is null) return;
            Socket clientSocket = new Socket(        // создаем сокет подключения
                AddressFamily.InterNetwork,   // адресация IPv4
                SocketType.Stream,            // Двусторонний сокет (и читать, и писать)
                ProtocolType.Tcp);            // Протокол сокета - ТСР
            try
            {
                clientSocket.Connect(endpoint);
                ClientRequest request = new ClientRequest()
                {
                    Action = "Get",
                    Author = authorTextBox.Text,
                    Moment = lastSyncMoment
                };
                lastSyncMoment = DateTime.Now;
                // преобразуем объект в JSON

                // отправляем на сервер
                SendRequest(clientSocket, request);

                var response = GetServerResponse(clientSocket);

                if (response is not null && response.Messages is not null)
                {
                    foreach (var message in response.Messages)
                    {
                        //chatLogs.Text += $"{message.Moment.ToShortTimeString()} {message.Author}: {message.Text}\n";
                        Label messageLabel = new()
                        {
                            Content = message.Text,
                            Background = Brushes.Lime,
                            Margin = new Thickness(10, 5, 10, 5),
                            HorizontalAlignment = HorizontalAlignment.Left
                        };
                        chatContainer.Children.Add(messageLabel);
                    }
                }
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Dispose();
                await Task.Delay(1000);
                ReCheckMessages();
            }
            catch (Exception ex)
            {
                chatLogs.Text += ex.Message + "\n";
            }


        }

        private void SendRequest(Socket clientSocket, ClientRequest request)
        {
            String json = JsonSerializer.Serialize(request,           // Для Юникода в JSON
                  new JsonSerializerOptions()
                  {                         // используются \uXXXX
                      Encoder = System.Text.Encodings.Web               // выражения. Чтобы 
                      .JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                  });  // был обычный текст - Encoder
            clientSocket.Send(Encoding.UTF8.GetBytes(json));
        }

        private ServerResponse? GetServerResponse(Socket clientSocket)
        {
            MemoryStream stream = new();               // Другой способ получения
            do                                         // данных - собирать части
            {                                          // бинарного потока в 
                int n = clientSocket.Receive(buffer);  // память.
                stream.Write(buffer, 0, n);            // Затем создать строку
            } while (clientSocket.Available > 0);      // один раз пройдя
            String str = Encoding.UTF8.GetString(      // все полученные байты.
                stream.ToArray());

            return JsonSerializer.Deserialize<ServerResponse>(str);
        }


    }
}
