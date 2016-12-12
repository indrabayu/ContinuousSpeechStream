using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2.Google.Vision
{
    public class VisionUtils
    {
        public static async Task<annotate> AnnotateImagesAsync(byte[] byteArray)
        {
            string content = Convert.ToBase64String(byteArray);

            string requestUrl = $"https://vision.googleapis.com/v1/images:annotate?key={Keys.GoogleVision}";
            const string template_analyze = "{\"requests\":[{\"image\":{\"content\":\"@content\"},\"features\":[{\"type\":\"TYPE_UNSPECIFIED\",\"maxResults\":50},{\"type\":\"LANDMARK_DETECTION\",\"maxResults\":50},{\"type\":\"FACE_DETECTION\",\"maxResults\":50},{\"type\":\"LOGO_DETECTION\",\"maxResults\":50},{\"type\":\"LABEL_DETECTION\",\"maxResults\":50},{\"type\":\"TEXT_DETECTION\",\"maxResults\":50},{\"type\":\"SAFE_SEARCH_DETECTION\",\"maxResults\":50},{\"type\":\"IMAGE_PROPERTIES\",\"maxResults\":50}]}]}";
            string param = template_analyze.Replace("@content", content);
            string json = await ExecuteUsing_HttpClient(requestUrl, param);
            if (string.IsNullOrEmpty(json))
                return null;
            var annotations = JsonConvert.DeserializeObject<annotate>(json);

            return annotations;
        }

        private static async Task<string> ExecuteUsing_HttpClient(string requestUrl, string param)
        {
            string json_result = string.Empty;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
                httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, br");

                httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
                httpClient.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                httpClient.DefaultRequestHeaders.UserAgent.Clear();
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://cloud.google.com/vision/");

                var uri = new Uri(requestUrl);

                var result = await httpClient.PostAsync(uri, new StringContent(param));

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    using (var zippedStream = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        using (var sr = new StreamReader(zippedStream))
                        {
                            json_result = sr.ReadToEnd();
                        }
                    }
                }
            }

            return json_result;
        }
    }
}