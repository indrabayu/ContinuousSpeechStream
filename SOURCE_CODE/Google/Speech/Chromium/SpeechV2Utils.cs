using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2.Google.Speech.Chromium
{
    public class SpeechV2Utils
    {
        const string Beginning = "{\"result\":[]}";

        public static string ParseJson(string json)
        {
            json = json.Replace(Beginning, string.Empty).Trim();

            if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                return string.Empty;

            if (json.Equals("\n") || json.Equals(Environment.NewLine))
                return string.Empty;

            string transcript = "\"transcript\":\"";
            int idx_transcript = json.IndexOf(transcript);
            string confidence = "\",\"confidence";
            int idx_confidence = json.IndexOf(confidence);
            string closing = "\"}";
            int idx_closing = json.IndexOf(closing);

            if (idx_confidence != -1)
            {
                return json.Substring(idx_transcript + transcript.Length, idx_confidence - idx_transcript - transcript.Length);
            }
            else if (idx_closing != -1)
            {
                return json.Substring(idx_transcript + transcript.Length, idx_closing - idx_transcript - transcript.Length);
            }
            else
            {
                return json;

                return
                    "ArgumentOutOfRangeException" + Environment.NewLine +
                    $"transcript = {transcript}" + Environment.NewLine +
                    $"idx_transcript = {idx_transcript}" + Environment.NewLine +
                    $"confidence = {confidence}" + Environment.NewLine +
                    $"idx_confidence = {idx_confidence}" + Environment.NewLine +
                    $"closing = {closing}" + Environment.NewLine +
                    $"idx_closing = {idx_closing}";
            }
        }

        /*
         * https://github.com/weed/spajam_gifu_2015/blob/master/%E6%9C%AC%E9%81%B8/SpajamHonsen/SpajamHonsen/Utilities/GoogleUtil.cs
         */
        public static async Task<string> RequestGoogleSpeechAPIAsync(byte[] byteArray)
        {
            string json = null;

            var uri = new Uri($"https://www.google.com/speech-api/v2/recognize?output=json&lang={GoogleSettings.SpeechLanguage}&key={Keys.ChromiumSpeechV2}");

            using (var ms = new MemoryStream(byteArray, 0, byteArray.Length))
            {
                using (var param = new StreamContent(ms))
                {
                    //var mediaType = new MediaTypeWithQualityHeaderValue("audio/x-flac");
                    var mediaType = new MediaTypeWithQualityHeaderValue("audio/l16");
                    var parameter = new NameValueHeaderValue("rate", GoogleSettings.SampleRate);
                    mediaType.Parameters.Add(parameter);
                    param.Headers.ContentType = mediaType;

                    using (var httpClient = new HttpClient())
                    {
                        using (var result = await httpClient.PostAsync(uri, param))
                        {
                            json = await result.Content.ReadAsStringAsync();
                        }
                    }
                }
            }

            return json;
        }

        public static string Send(FileType fileType, string filePath)
        {
            string result = string.Empty;

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                string requestUrl = $"https://www.google.com/speech-api/v2/recognize?output=json&lang=en-us&key={Keys.ChromiumSpeechV2}&client=chromium&maxresults=6&pfilter=2";
                var request = (HttpWebRequest)WebRequest.Create(requestUrl);
                ConfigureRequest(request, fileType);
                var requestStream = request.GetRequestStream();

                //CopyStream(fileStream, requestStream);
                fileStream.CopyTo(requestStream);

                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                result = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static void CopyStream(FileStream fileStream, Stream requestStream)
        {
            var buffer = new byte[32768];
            int read;
            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                requestStream.Write(buffer, 0, read);
            }
        }

        private static void ConfigureRequest(HttpWebRequest request, FileType fileType)
        {
            request.KeepAlive = true;
            request.SendChunked = true;
            if (fileType == FileType.Flac)
                request.ContentType = "audio/x-flac; rate=44100";
            else if (fileType == FileType.Wav)
                request.ContentType = "audio/l16; rate=16000";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
            request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip,deflate,sdch");
            request.Headers.Set(HttpRequestHeader.AcceptLanguage, "en-GB,en-US;q=0.8,en;q=0.6");
            request.Headers.Set(HttpRequestHeader.AcceptCharset, "ISO-8859-1,utf-8;q=0.7,*;q=0.3");
            request.Method = "POST";
        }
    }
    public enum FileType
    {
        Wav, // [Mono] [16000 Hz] [16 bit]
        Flac // [Mono] [44100 Hz] [16 bit]
    }
}