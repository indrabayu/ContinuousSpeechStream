using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App2.Bing.Map
{
    public class MapUtils
    {
        public static async Task<List<Tuple<string, string, string>>> GetLocations(string query, double la, double lo)
        {
            var baseAddress = new Uri("https://www.bing.com/");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    //usually i make a standard request without authentication, eg: to the home page.
                    //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                    var homePageResult = client.GetAsync("/");
                    homePageResult.Result.EnsureSuccessStatusCode();

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.AcceptEncoding.Clear();
                    client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, sdch, br");

                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");

                    client.DefaultRequestHeaders.Referrer = new Uri("https://www.bing.com/");
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    HttpResponseMessage loginResult;
                    double la1 = la - 0.05;
                    double lo1 = lo - 0.05;
                    double la2 = la + 0.05;
                    double lo2 = lo + 0.05;
                    loginResult = await client.GetAsync($"/mapspreview/overlay?q={query}&filters=direction_partner%3A%22maps%22%20tid%3A%22851415E615514659A88F8D18C72EA556%22&mapcardtitle={query}&appid=E18E19EF-764F-41A9-B53E-6E98AE519695&p1=[AplusAnswer%20ShownYPIDs=%22%22]&count=20&first=0&localMapView={la1},{lo1},{la2},{lo2}");
                    loginResult.EnsureSuccessStatusCode();

                    //make the subsequent web requests using the same HttpClient object
                    string DOM = string.Empty;
                    using (var responseStream = await loginResult.Content.ReadAsStreamAsync())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                DOM = sr.ReadToEnd();
                            }
                        }
                    }
                    return ParseBingMap(DOM);
                }
            }
        }

        static List<Tuple<string, string, string>> ParseBingMap(string DOM)
        {
            const string separator = "class=\"b_factrow\">";
            const char separator_char = '|';
            var specificDOM = DOM.Split(new string[] { "</style>" }, StringSplitOptions.RemoveEmptyEntries)[1];
            specificDOM = specificDOM.Replace(separator, separator + separator_char);
            specificDOM = specificDOM.Replace("&amp;", "&");
            var everything = StripHtml(specificDOM);
            var lines = everything.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var entries = new List<Tuple<string, string, string>>();
            for (int i = 1; i < lines.Length - 1;)
            {
                int _;
                if (i + 2 < lines.Length && int.TryParse(lines[i + 2][0] + "", out _))
                {
                    entries.Add(new Tuple<string, string, string>(lines[i], lines[i + 1], lines[i + 2].Replace(' ', '-')));
                    i += 3;
                }
                else
                {
                    entries.Add(new Tuple<string, string, string>(lines[i], lines[i + 1], "N/A"));
                    i += 2;
                }
            }
            return entries;
        }

        public static string StripHtml(string source)
        {
            string output;

            //get rid of HTML tags
            output = Regex.Replace(source, "<[^>]*>", string.Empty);

            //get rid of multiple blank lines
            output = Regex.Replace(output, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

            return output;
        }
    }
}