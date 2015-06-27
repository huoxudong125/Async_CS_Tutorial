using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WhenAny_EarlyBailout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource m_cts;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_cts != null) m_cts.Cancel();
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            m_cts = new CancellationTokenSource();
            startButton.IsEnabled = false;
            try
            {
                var imageDownload = AccessTheWebAsync();
                await UntilCompletionOrCancellation(imageDownload, m_cts.Token);
                if (imageDownload.IsCompleted)
                {
                    int length = await imageDownload;
                    resultsTextBox.Text +=
                String.Format("\r\n download length is {0}.\r\n", length);
                }
                else
                {
                    resultsTextBox.Text += String.Format("\r\n download failed or canceled.\r\n");
                }
            }
            finally { startButton.IsEnabled = true; }
        }

        ///The function is refernce from  article written by Stephen Toub, Microsoft
        ///but not the function is not fit to .netframework 4.5
        //    private static async Task UntilCompletionOrCancellation(
        //Task asyncOp, CancellationToken ct)
        //    {
        //        var tcs = new TaskCompletionSource<bool>();
        //        using (ct.Register(() => tcs.TrySetResult(true)))
        //            await Task.WhenAny(asyncOp, tcs.Task);
        //        return asncOp;
        //    }

        private static async Task<T> UntilCompletionOrCancellation<T>(
            Task<T> asyncOp, CancellationToken ct)
        {
            T result;
            var tcs = new TaskCompletionSource<T>();
            using (ct.Register(() => tcs.TrySetResult(default(T))))
            { result = await Task.WhenAny(asyncOp, tcs.Task).Unwrap(); }
            return result;
        }

        private async Task<int> AccessTheWebAsync()
        {
            HttpClient client = new HttpClient();

            resultsTextBox.Text +=
                String.Format("\r\nReady to download.\r\n");

            // You might need to slow things down to have a chance to cancel.
            await Task.Delay(250);

            // GetAsync returns a Task<HttpResponseMessage>.
            // ***The ct argument carries the message if the Cancel button is chosen.
            HttpResponseMessage response = await client.GetAsync("http://msdn.microsoft.com/en-us/library/dd470362.aspx");

            // Retrieve the website contents from the HttpResponseMessage.
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            // The result of the method is the length of the downloaded web site.
            return urlContents.Length;
        }
    }
}