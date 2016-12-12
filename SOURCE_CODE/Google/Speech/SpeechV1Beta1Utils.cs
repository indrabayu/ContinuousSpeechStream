using Newtonsoft.Json;
using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2.Google.Speech
{
    public class SpeechV1Beta1Utils
    {
        /*
         * https://github.com/weed/spajam_gifu_2015/blob/master/%E6%9C%AC%E9%81%B8/SpajamHonsen/SpajamHonsen/Utilities/GoogleUtil.cs
         */
        public static async Task<SpeechV1Beta1Results> RequestGoogleSpeechAPIAsync(byte[] byteArray, params string[] newWords)
        {
            string content = Convert.ToBase64String(byteArray);

            string requestUrl = $"https://speech.googleapis.com/v1beta1/speech:syncrecognize?key={Keys.GoogleSpeechV1Beta1}";
            const string template_analyze = "{\"config\":{\"encoding\":\"LINEAR16\",\"sampleRate\":@sampleRate,\"languageCode\":\"@languageCode\",\"maxAlternatives\":0,\"profanityFilter\":false,\"speech_context\": {\"phrases\":[@speech_context]}},\"audio\":{\"content\":\"@content\"}}";
            string param = template_analyze.Replace("@sampleRate", GoogleSettings.SampleRate).Replace("@languageCode", GoogleSettings.SpeechLanguage).Replace("@content", content)
                .Replace("@speech_context", newWords == null || newWords.Length == 0 ? string.Empty : newWords.Select(_ => '\"' + _ + '\"').Aggregate((a, b) => a + ',' + b));
            string json = await ExecuteUsing_HttpClient(requestUrl, param);
            if (string.IsNullOrEmpty(json))
                return null;
            var _utterance = JsonConvert.DeserializeObject<SpeechV1Beta1Results>(json);

            return _utterance;
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

                httpClient.DefaultRequestHeaders.Referrer = new Uri("https://cloud.google.com/speech/");

                var result = await httpClient.PostAsync(new Uri(requestUrl), new StringContent(param));

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
        const string strUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36";
    }
}