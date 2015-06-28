using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using AsyncLibraryDemo;

namespace WhenAny_Interleaving
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
            cts = new CancellationTokenSource();
            var urls = new List<string>
            {
                "https://ss0.bdstatic.com/5aV1bjqh_Q23odCf/static/superplus/img/logo_white_ee663702.png",
                "https://assets-cdn.github.com/images/icons/pridetocat.png",
                "https://download-codeplex.sec.s-msft.com/Download?ProjectName=wix&DownloadId=119157&Build=21018",
                "http://b.zol-img.com.cn/desk/bizhi/image/3/960x600/1369966956468.jpg",
                "http://b.zol-img.com.cn/desk/bizhi/image/3/960x600/136858489885.jpg",
                "http://www.qqwind.com/upload/2010/7/31/20107311904131568.jpg",
                "http://www.51pptmoban.com/d/file/2014/06/08/4da6bfff66f923cd3b29f6fb10abdf55.jpg",
                "https://g.wen.lu/!encrypted-tbn1.gstatic.com/images?q=tbn:ANd9GcQLXnzZ_7_vDBlCZGWCqNS5cqPNlUWmxzIVRrDW5zklSXMkj5bz"
            };

            List<Task<BitmapImage>> imageTasks =
            (from imageUrl in urls select GetBitmapAsync(imageUrl)).ToList();


            resultsTextBox.Clear();
            panel.Children.Clear();
            resultsTextBox.Text = "Begin to download images.";
            

            while (imageTasks.Count > 0)
            {
                try
                {
                    
                    Task<BitmapImage> imageTask = await Task.WhenAny(imageTasks);
                    imageTasks.Remove(imageTask);

                    resultsTextBox.Text += string.Format("\r\n Task[{0}] is completed.", imageTask.Id);

                    var image = await imageTask;
                    var imageControl = new System.Windows.Controls.Image() { Source = image };
                    panel.Children.Add(imageControl);
                }
                catch {
                    resultsTextBox.Text += string.Format("\r\n this is an exception.");
                }
            }

            resultsTextBox.Text += "\r\nEnd to download images.";
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private async Task<BitmapImage> GetBitmapAsync(string imageUrl)
        {
            var imageData = await WebAsyncUtility.AccessTheImageContent(imageUrl, CancellationToken.None);
            return imageData.ToBitmap();
        }

        private async Task<Bitmap> ConvertImage(Bitmap bitmap)
        {
            return bitmap;
        }
    }
}