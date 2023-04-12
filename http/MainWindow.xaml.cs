using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace http
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient httpClient;
        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void getButton1_Click(object sender, RoutedEventArgs e)
        {
            String result = await httpClient.GetStringAsync(urlTextBox1.Text);
            resultTextBlock.Text = result;

        }

        private async void getButton2_Click(object sender, RoutedEventArgs e)
        {
            HttpRequestMessage request = new(HttpMethod.Get, urlTextBox1.Text);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            PrintResponse(response);

        }

        private async void HeadButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, urlTextBox3.Text));
            PrintResponse(response);
        }
        private async void PrintResponse(HttpResponseMessage response)
        {
            resultTextBlock.Text = $"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}\n";

            foreach (var header in response.Headers)
            {
                String headerString = header.Key + ": ";
                foreach (string value in header.Value)
                {
                    headerString += value + " ";
                }
                resultTextBlock.Text += $"{headerString}\n";
            }
            resultTextBlock.Text += $"-----------------------------------\n";
            resultTextBlock.Text += await response.Content.ReadAsStringAsync();
            resultTextBlock.Text += $"\n-----------------------------------\n";
        }

        private async void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Options, urlTextBox4.Text));
            PrintResponse(response);
        }

        private async void GetButton5_Click(object sender, RoutedEventArgs e)
        {
            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, urlTextBox5.Text));
            PrintResponse(response);
            var htmlCode = await response.Content.ReadAsStringAsync();

            var regex = new Regex(@"<li>(\w{8})</li>");
            var matches = regex.Matches(htmlCode);

            var passwordsText = "";

            foreach (Match match in matches)
            {
                passwordsText += match.Groups[1].Value + "\n";
            }

            resultTextBlock.Text += passwordsText;
        }
    }
}
