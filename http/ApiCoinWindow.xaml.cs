using System;
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
    /// Логика взаимодействия для ApiCoinWindow.xaml
    /// </summary>
    public partial class ApiCoinWindow : Window
    {
        public ObservableCollection<Asset> Assets { get; set; }
        private HttpClient _httpClient = new();
        private int indexShown;
        public ApiCoinWindow()
        {
            InitializeComponent();
            Assets = new();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAssets();
            
        }

        private async void LoadAssets()
        {
            var AssetsResponse = await _httpClient.GetFromJsonAsync<AssetsResponse>("https://api.coincap.io/v2/assets");
            if(AssetsResponse is null) 
            { 
                MessageBox.Show("json error");
                return;
            }
            indexShown = 0;
            ShowHistory(AssetsResponse.data[indexShown]);
            Assets.Clear();
            foreach(Asset asset in AssetsResponse.data)
            {
                Assets.Add(asset);
            }
            AssetsListView.SelectedIndex = indexShown;

        }
        private void DrawLine(double fromX,double fromY,double toX,double toY)
        {
            Line line = new()
            {
                X1 = fromX,
                Y1 = fromY,
                X2 = toX,
                Y2 = toY,
                Stroke = new SolidColorBrush(Colors.BlueViolet),
                StrokeThickness = 1
            };
            GraphCanvas.Children.Add(line);
        }
        private async void ShowHistory(Asset asset)
        {
            String url = $"https://api.coincap.io/v2/assets/{asset.id}/history?interval=d1";
            var historyResponse = await _httpClient.GetFromJsonAsync<HistoryResponse>(url);
            if(historyResponse is null)
            {
                MessageBox.Show("json history error");
                return;
            }

            long minTime;
            long maxTime;
            double minRate;
            double maxRate;

            minTime = historyResponse.data.Min(r => r.time);
            maxTime = historyResponse.data.Max(r => r.time);
            minRate = historyResponse.data.Min(r => r.priceUsd);
            maxRate = historyResponse.data.Max(r => r.priceUsd);
            double graphHeight = GraphCanvas.ActualHeight;
            double graphWidth = GraphCanvas.ActualWidth;

            double x1;
            double x2;
            double y1;
            double y2;
            const double offset = 0.05;
            double h = graphHeight * (1 - offset);
            double kx = graphWidth / (maxTime - minTime);
            double ky = graphHeight / (maxRate - minRate) *(1-2*offset);
           
            x1 = (historyResponse.data[0].time - minTime) * kx ;
            y1= h - (historyResponse.data[0].priceUsd - minRate) * ky;
            foreach(Rate rate in historyResponse.data)
            {
                x2=(rate.time - minTime) * kx;
                y2 = h - (rate.priceUsd - minRate) * ky;
                DrawLine(x1,y1, x2, y2);
                x1 = x2;
                y1 = y2;

            }
        }

        private void AssetsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(indexShown == AssetsListView.SelectedIndex)
            {
                return;
            }
            MessageBox.Show(AssetsListView.SelectedIndex.ToString());
        }
    }
    public class Asset
    {

        public string id { get; set; }

        public string rank { get; set; }

        public string symbol { get; set; }

        public string name { get; set; }

        public string supply { get; set; }

        public string maxSupply { get; set; }

        public string marketCapUsd { get; set; }

        public string volumeUsd24Hr { get; set; }

        public string priceUsd { get; set; }

        public string changePercent24Hr { get; set; }

        public string vwap24Hr { get; set; }

        public string explorer { get; set; }
    }

    public class AssetsResponse
    {
        public List<Asset> data { get; set; }
        public long timestamp { get; set; }
    }

    public class Rate
    {
        public double priceUsd { get; set; }
        public long time { get; set; }
        public DateTime date { get; set; }
    }
    public class HistoryResponse
    {
        public List<Rate> data { get; set; }
        public long timestamp { get; set; }
    }

}
