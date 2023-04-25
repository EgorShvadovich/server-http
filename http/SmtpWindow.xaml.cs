using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
using System.Windows.Shapes;
using http.Data;

namespace http
{
    /// <summary>
    /// Логика взаимодействия для SmtpWindow.xaml
    /// </summary>
    public partial class SmtpWindow : Window
    {
        private dynamic? emailConfig;
        private readonly DataContext _dataContext;
        public SmtpWindow()
        {
            InitializeComponent();
            _dataContext = new DataContext();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /* Открываем файл конфигурации и пытаемся извлечь данные */
            String configFilename = "emailconfig.json";
            try
            {
                // для универсальности используем динамический тип для данных
                emailConfig = JsonSerializer.Deserialize<dynamic>(
                    System.IO.File.ReadAllText(configFilename)
                );
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show($"Не найден файл конфигурации '{configFilename}'");
                this.Close();
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Ошибка преобразования конфигурации '{ex.Message}'");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обработки конфигурации '{ex.Message}'");
                this.Close();
            }
            if (emailConfig is null)
            {
                MessageBox.Show("Ошибка получения конфигурации");
                this.Close();
            }
        }

        private SmtpClient GetSmtpClient()
        {
            if (emailConfig is null) { return null!; }
            /* Динамические объекты позволяют разыменовывать свои поля как
             * emailConfig.GetProperty("smtp").GetProperty("gmail").GetProperty("host").GetString();
             */
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");

            String host = gmail.GetProperty("host").GetString()!;
            int port = gmail.GetProperty("port").GetInt32();
            String mailbox = gmail.GetProperty("email").GetString()!;
            String password = gmail.GetProperty("password").GetString()!;
            bool ssl = gmail.GetProperty("ssl").GetBoolean();

            return new(host)
            {
                Port = port,
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
        }

        private void SendTestButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            try
            {
                smtpClient.Send(
                    from: mailbox,
                    recipients: "egorplayer88@gmail.com",
                    subject: "Test Message",
                    body: "Test message from SmtpWindow");

                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }

        }

        private void SendHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").GetString()!;
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = "<u>Test</u> <i>message</i> from <b style='color:green'>SmtpWindow</b>",
                IsBodyHtml = true,
                Subject = "Test Message"
            };
            mailMessage.To.Add(new MailAddress("egorplayer88@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);
                MessageBox.Show("Sent OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }
        private void SendRandomCodeButton_Click(object sender, RoutedEventArgs e)
        {
            using SmtpClient smtpClient = GetSmtpClient();
            JsonElement gmail = emailConfig.GetProperty("smpt").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").ToString()!;
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = $"<b>Ваш случайный пароль из 8 символов: </b> <b style='color:red'>{RandomCode(8)}</b>",
                IsBodyHtml = true,
                Subject = "Test Message"

            };
            mailMessage.To.Add(new MailAddress("egorplayer88@gmail.com"));

            try
            {
                smtpClient.Send(mailMessage);

                MessageBox.Show("Sent OK!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sent error '{ex.Message}'");
            }
        }

        private String RandomCode(int x)
        {
            string chars = "abcdefghiklmnpqrstuvwxy0123456789";
            Random random = new Random();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < x; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }
            string randomString = builder.ToString();
            return randomString;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            String emailPattern = System.IO.File.ReadAllText("email.html");
            String confirmCode = RandomCode(6);
            String emailBody = emailPattern.Replace("*name*", UserNameTextbox.Text).Replace("*code*", confirmCode);

            //отправляем письмо с телом emailBody
            using SmtpClient smtpClient = GetSmtpClient(); 
            JsonElement gmail = emailConfig.GetProperty("smtp").GetProperty("gmail");
            String mailbox = gmail.GetProperty("email").ToString()!; MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(mailbox),
                Body = emailBody,
                IsBodyHtml = true,
                Subject = "Test Message"
            }; mailMessage.To.Add(new MailAddress("egorplayer88@gmail.com"));
            _dataContext.NPUsers.Add(new()
            {
                Id = Guid.NewGuid(),
                Name = UserNameTextbox.Text,
                Email = UserEmailTextbox.Text,
                ConfirmCode = confirmCode
            });
            smtpClient.Send(mailMessage);
            _dataContext.SaveChanges();
            ConfirmDockPanel.Visibility = Visibility.Visible;
            NPUser? user = _dataContext.NPUsers.FirstOrDefault(u => u.Name == UserNameTextbox.Text && u.Email == UserEmailTextbox.Text); if (user is null)
            {
                ConfirmDockPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                ConfirmDockPanel.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
