using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2.Google.Translate
{
    public class TranslateUtils
    {
        public static async Task<string> Translate(string statement, string langFrom, string langTo)
        {
            string json_result = string.Empty;

            var baseAddress = new Uri(strBaseAddress);
            //var cookieContainer = new CookieContainer();
            //using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(/*handler*/) { BaseAddress = baseAddress })
                {
                    //usually i make a standard request without authentication, eg: to the home page.
                    //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                    //var homePageResult = client.GetAsync("/");
                    //homePageResult.Result.EnsureSuccessStatusCode();

                    //client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(accept);

                    //client.DefaultRequestHeaders.AcceptEncoding.Clear();
                    client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd(strAcceptEncoding);

                    //client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd(strAcceptLanguage);

                    //client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(strUserAgent);

                    client.DefaultRequestHeaders.Referrer = Referrer;
                    client.DefaultRequestHeaders.Host = strHost;

                    HttpResponseMessage loginResult;
                    loginResult = await client.GetAsync($"/language/translate/v2?key={Keys.GoogleTranslate}&source={langFrom.ToLower()}&target={langTo.ToLower()}{PremiumEditionInfo}&q={statement.Replace(" ", "%20")}");
                    //loginResult.EnsureSuccessStatusCode();

                    using (var stream = await loginResult.Content.ReadAsStreamAsync())
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
            }

            const string apostrophe = "&#39;";
            if (json_result.Contains(apostrophe)) /* apostrophe */
                json_result = json_result.Replace(apostrophe, "'");

            return json_result;
        }

        const string strBaseAddress = "https://cloud.google.com/translate/";
        static readonly MediaTypeWithQualityHeaderValue accept = new MediaTypeWithQualityHeaderValue("application/json");
        const string strAcceptEncoding = "gzip, deflate, sdch, br";
        const string strAcceptLanguage = "en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2";
        const string strUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
        static readonly Uri Referrer = new Uri("https://cloud.google.com/translate/");
        static readonly string strHost = "www.googleapis.com";
        const bool UsePremiumEdition = true;
        static string PremiumEditionInfo { get { return UsePremiumEdition ? "&model=nmt" : string.Empty; } }
    }
}