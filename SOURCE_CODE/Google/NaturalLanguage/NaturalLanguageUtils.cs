using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace App2.Google.NaturalLanguage
{
    public class NaturalLanguageUtils
    {
        //static void Execute()
        //{
        //    //string statement = "Word completion is a user interface feature that offers the user a list of words after one or more letters have been typed. It has been around as a feature of word editors and command shells for nearly half a century. In information retrieval, word completion is best known in the form of query auto completion, which provides users with suggested queries as they begin to enter their query in the search box. The user’s incomplete input is often called a query prefix and the suggested queries are often called query completions.";
        //    //string statement = "My name is not important.";
        //    //string statement = "Google, headquartered in Mountain View, unveiled the new Android phone at the Consumer Electronic Show.  Sundar Pichai said in his keynote that users love their new Android phones.";
        //    string statement = "Axl Rose is coming to Jakarta using Brussels Air. And he plans to drink Heineken while playing game on PS4. Maybe Resident Evil 2, as Jill Valentine?";
        //    //Console.WriteLine(Execute_WebRequest(statement).Item1);
        //    //Console.WriteLine(Execute_HttpClient(statement).Result.Item1);

        //    /* HttpClient */
        //    Console.WriteLine(Analyze_and_Annotate(statement, analyzeEntities: true, analyzeSentiment: false, annotateText: true, useHttpClient: true).Result[0]);

        //    /* HttpWebRequest */
        //    Console.WriteLine(Analyze_and_Annotate(statement, analyzeEntities: true, analyzeSentiment: false, annotateText: true, useHttpClient: false).Result[0]);
        //}

        private static async Task<List<string>> Analyze_and_Annotate(string statement, bool analyzeEntities, bool analyzeSentiment, bool annotateText, bool useHttpClient)
        {
            Func<string, Task<string>> _ = async (command) =>
            {
                string json_result = string.Empty;

                string requestUrl = $"https://language.googleapis.com/v1/documents:{command}?key={Keys.GoogleNaturalLanguage}";

                const string template_analyze = "{\"document\":{\"type\":\"PLAIN_TEXT\",\"content\":\"@statement\"},\"encodingType\":\"UTF16\"}";
                const string template_annotate = "{\"document\":{\"type\":\"PLAIN_TEXT\",\"content\":\"@statement\"},\"features\":{\"extractSyntax\":true,\"extractEntities\":true,\"extractDocumentSentiment\":true}}";

                string param = string.Empty;
                if (command.Equals("analyzeEntities") || command.Equals("analyzeSentiment"))
                    param = template_analyze.Replace("@statement", statement);
                else if (command.Equals("annotateText"))
                    param = template_annotate.Replace("@statement", statement);

                if (useHttpClient)
                    json_result = await ExecuteUsing_HttpClient(requestUrl, param);
                else
                    json_result = await ExecuteUsing_HttpWebRequest(requestUrl, param);

                return json_result;
            };

            var list = new List<string>(new string[] { string.Empty, string.Empty, string.Empty });

            if (analyzeEntities) list[0] = await _("analyzeEntities");
            if (analyzeSentiment) list[1] = await _("analyzeSentiment");
            if (annotateText) list[2] = await _("annotateText");

            return list;
        }

        public static async Task<AnalyzeEntities.analyzeEntities> Execute_AnalyzeEntities(string statement)
        {
            string requestUrl = $"https://language.googleapis.com/v1/documents:analyzeEntities?key={Keys.GoogleNaturalLanguage}";
            const string template_analyze = "{\"document\":{\"type\":\"PLAIN_TEXT\",\"content\":\"@statement\"},\"encodingType\":\"UTF16\"}";
            string param = template_analyze.Replace("@statement", statement);
            string json = await ExecuteUsing_HttpClient(requestUrl, param);
            var _analyzeEntities = JsonConvert.DeserializeObject<AnalyzeEntities.analyzeEntities>(json);
            return _analyzeEntities;
        }

        public static async Task<AnalyzeSentiment.analyzeSentiment> Execute_AnalyzeSentiment(string statement)
        {
            string requestUrl = $"https://language.googleapis.com/v1/documents:analyzeSentiment?key={Keys.GoogleNaturalLanguage}";
            const string template_analyze = "{\"document\":{\"type\":\"PLAIN_TEXT\",\"content\":\"@statement\"},\"encodingType\":\"UTF16\"}";
            string param = template_analyze.Replace("@statement", statement);
            string json = await ExecuteUsing_HttpClient(requestUrl, param);
            var _analyzeSentiment = JsonConvert.DeserializeObject<AnalyzeSentiment.analyzeSentiment>(json);
            return _analyzeSentiment;
        }

        public static async Task<AnnotateText.annotateText> Execute_AnnotateText(string statement)
        {
            string requestUrl = $"https://language.googleapis.com/v1/documents:annotateText?key={Keys.GoogleNaturalLanguage}";
            const string template_annotate = "{\"document\":{\"type\":\"PLAIN_TEXT\",\"content\":\"@statement\"},\"features\":{\"extractSyntax\":true,\"extractEntities\":true,\"extractDocumentSentiment\":true}}";
            string param = template_annotate.Replace("@statement", statement);
            string json = await ExecuteUsing_HttpClient(requestUrl, param);
            var _annotateText = JsonConvert.DeserializeObject<AnnotateText.annotateText>(json);
            return _annotateText;
        }

        private static async Task<string> ExecuteUsing_HttpWebRequest(string requestUrl, string param)
        {
            string json_result = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(requestUrl);

            ConfigureRequest(request, param.Length);

            var data = Encoding.UTF8.GetBytes(param);
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
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

        private static void ConfigureRequest(HttpWebRequest request, int ContentLength)
        {
            request.KeepAlive = true;
            request.SendChunked = false;
            request.ContentType = "application/json"; // "application/json";// "text/plain;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2899.0 Safari/537.36";
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");
            request.Method = "POST";
            request.Referer = "https://cloud.google.com/natural-language/";
            request.ContentLength = ContentLength;
        }

        private static async Task<string> ExecuteUsing_HttpClient(string requestUrl, string param)
        {
            string json_result = string.Empty;

            using (var httpClient = new HttpClient())
            {
                //httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(accept);

                //httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
                httpClient.DefaultRequestHeaders.AcceptEncoding.TryParseAdd(strAcceptEncoding);

                //httpClient.DefaultRequestHeaders.AcceptLanguage.Clear();
                httpClient.DefaultRequestHeaders.AcceptLanguage.TryParseAdd(strAcceptLanguage);

                //httpClient.DefaultRequestHeaders.UserAgent.Clear();
                httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(strUserAgent);

                httpClient.DefaultRequestHeaders.Referrer = Referrer;

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

        static readonly MediaTypeWithQualityHeaderValue accept = new MediaTypeWithQualityHeaderValue("application/json");
        const string strAcceptEncoding = "gzip, deflate, br";
        const string strAcceptLanguage = "en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2";
        const string strUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2899.0 Safari/537.36";
        static readonly Uri Referrer = new Uri("https://cloud.google.com/natural-language/");
    }
}