using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AsyncLibraryDemo;

namespace WhenAny_Redundancy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            resultsTextBox.Clear();

            cts = new CancellationTokenSource();
            var recommendations = new List<Task<Tuple<string, bool>>>()
                {
                    GetBuyRecommendationAsync("http://g.wen.lu",cts.Token),
                    GetBuyRecommendationAsync("http://wwww.google.com00",cts.Token),
                    GetBuyRecommendationAsync("http://www.irocktech.com",cts.Token),
                    GetBuyRecommendationAsync("http://wwww.baidu0.com",cts.Token),
                };

            while (recommendations.Count > 0)
            {
                Task<Tuple<string, bool>> recommendation = await Task.WhenAny(recommendations);
                try
                {
                    var result = await recommendation;
                    if (result.Item2)
                    {
                        resultsTextBox.Text += string.Format("\r\n have buy a stock [{0}]", result.Item1);
                    }
                    cts.Cancel();
                    break;
                }
                catch (Exception exc)
                {
                    recommendations.Remove(recommendation);
                }
            }

            resultsTextBox.Text += "\r\n complete buy stock.\r\n";

            for (int i = 0; i < 10; i++)
            {
                resultsTextBox.Text += string.Format("\r\n=======[{0}]========", i);
                LogCompletionIfFailed(recommendations);
                await Task.Delay(1000);
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private async Task<Tuple<string, bool>> GetBuyRecommendationAsync(string url, CancellationToken ct)
        {
            var tempUrl = string.IsNullOrEmpty(url) ? "http://msdn.microsoft.com/en-us/library/hh290138.aspx" : url;

            var result = await WebAsyncUtility.AccessTheWebAsync(tempUrl, ct);

            return new Tuple<string, bool>(tempUrl, result > 0);
        }

        private async void LogCompletionIfFailed(IEnumerable<Task<Tuple<string, bool>>> tasks)
        {
            foreach (var task in tasks)
            {
                try
                {
                    //var result = await task;
                    resultsTextBox.Text += string.Format("\r\n {0} Status {1}", task.Id, task.Status);
                }
                catch (Exception exc)
                {
                    resultsTextBox.Text = exc.Message;
                }
            }
        }
    }
}