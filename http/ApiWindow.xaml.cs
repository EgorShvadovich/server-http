﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
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
    /// Логика взаимодействия для ApiWindow.xaml
    /// </summary>
    public partial class ApiWindow : Window
    {
        public ObservableCollection<NbuRate> NbuRates { get; set; }

        private HttpClient httpClient;
        private List<NbuRate>? _nbuRates;

        public ApiWindow()
        {
            InitializeComponent();
            httpClient = new();
            NbuRates = new();
            this.DataContext = this;
            // MessageBox.Show( 4.ToString("00") );
        }

        private async void NbuToday_Click(object sender, RoutedEventArgs e)
        {
            String url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";

            // String json = await httpClient.GetStringAsync(url);
            // var rates = JsonSerializer.Deserialize<List<NbuRate>>(json);

            _nbuRates = await httpClient.GetFromJsonAsync<List<NbuRate>>(url);

            if (_nbuRates is null)
            {
                MessageBox.Show("JSON parse error");
            }
            else
            {
                NbuRates.Clear();
                foreach (NbuRate rate in _nbuRates)
                {
                    NbuRates.Add(rate);
                }
                /*
                StringBuilder sb = new();
                foreach (NbuRate rate in rates)
                {
                    sb.Append(rate.txt).Append(' ').Append(rate.rate).Append('\n');
                }
                resultTextBlock.Text = sb.ToString();
                */
            }
        }

        private void ListView_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header
                && header.Content is not null
                && _nbuRates is not null)
            {
                _nbuRates = header.Content.ToString() switch
                {
                    "cc" => _nbuRates.OrderBy(r => r.cc).ToList(),
                    "txt" => _nbuRates.OrderBy(r => r.txt).ToList(),
                    "rate" => _nbuRates.OrderBy(r => r.rate).ToList(),
                    "r030" => _nbuRates.OrderBy(r => r.r030).ToList(),
                    _ => _nbuRates
                };

                NbuRates.Clear();
                foreach (NbuRate rate in _nbuRates)
                {
                    NbuRates.Add(rate);
                }
                // MessageBox.Show(header.Content.ToString());
            }
            /* Д.З. Реализовать элемент выбора даты и вывести курсы валют
             * из API НБУ на выбранную дату
             * https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date=20200302&json
             */
        }

        private async void NbuOnDate_Click(object sender, RoutedEventArgs e)
        {
            int day = Convert.ToInt32(DateDay.Text);
            int month = Convert.ToInt32(DateMonth.Text);
            String url = $"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date={DateYear.Text}{DateDay.Text}{DateMonth}&json";
            if (day <= 9 && day >= 1)
            {
                string dayString = day.ToString("00");
                url = $"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date={DateYear.Text}{dayString}{DateMonth.Text}&json";

            }
            if (month <= 9 && month >= 1)
            {
                string monthString = month.ToString("00");
                url = $"https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?date={DateYear.Text}{DateDay.Text}{monthString}&json";

            }
            // String json = await httpClient.GetStringAsync(url);
            // var rates = JsonSerializer.Deserialize<List<NbuRate>>(json);

            _nbuRates = await httpClient.GetFromJsonAsync<List<NbuRate>>(url);

            if (_nbuRates is null)
            {
                MessageBox.Show("JSON parse error");
            }
            else
            {
                NbuRates.Clear();
                foreach (NbuRate rate in _nbuRates)
                {
                    NbuRates.Add(rate);
                }
                /*
                StringBuilder sb = new();
                foreach (NbuRate rate in rates)
                {
                    sb.Append(rate.txt).Append(' ').Append(rate.rate).Append('\n');
                }
                resultTextBlock.Text = sb.ToString();
                */
            }
        }
    }

    // ORM - объектное отображение - представление данных в виде объектов и их коллекций
    /* [
          {
            "r030": 36,
            "txt": "Австралійський долар",
            "rate": 24.6326,
            "cc": "AUD",
            "exchangedate": "14.04.2023"
          },...
    Массив объектов
    */
    public class NbuRate
    {
        public int r030 { get; set; }
        public String txt { get; set; }
        public Double rate { get; set; }
        public String cc { get; set; }
        public String exchangedate { get; set; }
    }
}
