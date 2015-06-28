using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncLibraryDemo
{
    public class WebAsyncUtility
    {
        public static readonly string FirstDemoUrl = "http://msdn.microsoft.com/en-us/library/dd470362.aspx";

        //
        public static async Task<int> AccessTheWebContentLengthAsync(string url)
        {
            HttpClient client = new HttpClient();

            //TODO: In fact in practice work we don''t need it,wen can using early bailout pattern.
            // You might need to slow things down to have a chance to cancel.
            await Task.Delay(250);

            // GetAsync returns a Task<HttpResponseMessage>.
            // ***The ct argument carries the message if the Cancel button is chosen.
            HttpResponseMessage response = await client.GetAsync(url);

            // Retrieve the website contents from the HttpResponseMessage.
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            // The result of the method is the length of the downloaded web site.
            return urlContents.Length;
        }

        public static async Task<int> AccessTheWebContentLengthAsync(string url, CancellationToken ct)
        {
            HttpClient client = new HttpClient();

            var ran = new Random();

            // You might need to slow things down to have a chance to cancel.
            await Task.Delay((int)(ran.NextDouble() * 10000));

            // GetAsync returns a Task<HttpResponseMessage>.
            // ***The ct argument carries the message if the Cancel button is chosen.
            HttpResponseMessage response = await client.GetAsync(url, ct);

            // Retrieve the website contents from the HttpResponseMessage.
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            // You might need to slow things down to have a chance to cancel.
            await Task.Delay((int)(ran.NextDouble() * 10000));

            // The result of the method is the length of the downloaded web site.
            return urlContents.Length;
        }


        public static async Task<Byte[]> AccessTheImageContent(string imageUrl, CancellationToken ct)
        {
            HttpClient client = new HttpClient();

            // GetAsync returns a Task<HttpResponseMessage>.
            // ***The ct argument carries the message if the Cancel button is chosen.
            HttpResponseMessage response = await client.GetAsync(imageUrl, ct);

            // Retrieve the website contents from the HttpResponseMessage.
            byte[] urlContents = await response.Content.ReadAsByteArrayAsync();

            return urlContents;
        }

        // ***Add a method that creates a list of web addresses.
        public static List<string> SetUpURLList()
        {
            List<string> urls = new List<string>
            {
                "http://msdn.microsoft.com",
                "http://msdn.microsoft.com/en-us/library/hh290138.aspx",
                "http://msdn.microsoft.com/en-us/library/hh290140.aspx",
                "http://msdn.microsoft.com/en-us/library/dd470362.aspx",
                "http://msdn.microsoft.com/en-us/library/aa578028.aspx",
                "http://msdn.microsoft.com/en-us/library/ms404677.aspx",
                "http://msdn.microsoft.com/en-us/library/ff730837.aspx"
            };
            return urls;
        }
    }
}