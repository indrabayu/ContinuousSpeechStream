using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2.Google.Search
{
    public class SearchUtils
    {
        public static async Task<string> GetSuggestionForMispelling(string query)
        {
            var baseAddress = new Uri("https://www.google.co.id/");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    query = query.Replace(" ", "%20");

                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.AcceptEncoding.Clear();
                    client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, sdch, br");

                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");

                    //client.DefaultRequestHeaders.Referrer = new Uri("https://www.google.co.id/");
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    var homePageResult = client.GetAsync($"/search?sourceid=chrome-psyapi2&ion=1&espv=2&ie=UTF-8&q={query}&oq={query}");
                    //homePageResult.Result.EnsureSuccessStatusCode();

                    string DOM;// = await homePageResult.Result.Content.ReadAsStringAsync();

                    using (var responseStream = await homePageResult.Result.Content.ReadAsStreamAsync())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                DOM = sr.ReadToEnd();
                            }
                        }
                    }

                    const string showing_results_for = "Showing results for</span>";
                    int indexOf_showing_results_for = DOM.IndexOf(showing_results_for);
                    if (indexOf_showing_results_for == -1)
                        return null;

                    Func<string, string> extractSuggestion = (string document) =>
                    {
                        int begin = 0, end = 0;

                        string toSearch = "\">";
                        begin = document.IndexOf(toSearch, indexOf_showing_results_for);
                        begin += toSearch.Length;
                        end = document.IndexOf("</a>", begin);

                        string suggestion = document.Substring(begin, end - begin);
                        if (suggestion.StartsWith("Did you mean ") && suggestion.Contains(" instead of"))
                        {
                            begin = "Did you mean ".Length;
                            end = suggestion.IndexOf(" instead of");

                            suggestion = suggestion.Substring(begin, end - begin);
                        }
                        return suggestion;
                    };

                    return extractSuggestion(DOM);
                }
            }
        }

        public static async Task<string> Get_IamFeelingLucky_URL(string query, bool useMonkeyTricks)
        {
            var baseAddress = new Uri("https://www.google.com/");
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
                    if (useMonkeyTricks)
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        client.DefaultRequestHeaders.AcceptEncoding.Clear();
                        client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, sdch, br");

                        client.DefaultRequestHeaders.AcceptLanguage.Clear();
                        client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                        client.DefaultRequestHeaders.UserAgent.Clear();
                        client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2903.0 Safari/537.36");

                        client.DefaultRequestHeaders.Referrer = new Uri("https://www.google.co.id/");
                    }
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    HttpResponseMessage loginResult;
                    loginResult = await client.GetAsync($"/search?num=100&site=&source=hp&q={query}&oq={query}&gs_l=hp.7..0l10.1400.5249.0.46126.18.16.2.0.0.0.174.1769.3j12.15.0....0...1.1.64.hp..1.16.1610.0..0i131k1j0i3k1j0i10k1.YFk_wFBGySc&btnI=1");
                    //loginResult = await client.GetAsync($"/search?num=100&site=&source=hp&q={formattedQuery}&oq={formattedQuery}&btnI=1");
                    loginResult.EnsureSuccessStatusCode();

                    //make the subsequent web requests using the same HttpClient object
                    return loginResult.RequestMessage.RequestUri.ToString();
                }
            }
        }

        public static async Task<List<string>> GetCompletions(string query)
        {
            var baseAddress = new Uri("https://www.google.com");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    client.DefaultRequestHeaders.Accept.Clear();
                    MediaTypeWithQualityHeaderValue media;
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("text/html;q=0.8", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("application/xhtml+xml;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("application/xhtml+xml;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("application/xml;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("image/webp;q=0.8", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("*/*;q=0.8", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);

                    client.DefaultRequestHeaders.AcceptEncoding.Clear();
                    client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, sdch, br");

                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    //usually i make a standard request without authentication, eg: to the home page.
                    //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                    var homePageResult = client.GetAsync("/");
                    homePageResult.Result.EnsureSuccessStatusCode();

                    string DOM;

                    using (var responseStream = await homePageResult.Result.Content.ReadAsStreamAsync())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                DOM = sr.ReadToEnd();
                            }
                        }
                    }

                    HttpResponseMessage postResult;
                    string param_client = GetCompletions_GetValueOf(DOM, "client"); //the key is "client"
                    string param_hl = GetCompletions_GetValueOf(DOM, "hl"); //the key is "hl"
                                                                            //string param_token = GetValueOf(DOM, "token"); //the key is "tok" and not "token"
                    postResult = await client.GetAsync($"/complete/search?client={param_client}&hl={param_hl}&gs_rn=64&gs_ri=hp&cp={query.Length}&gs_id=12&q={query.Replace(" ", " %20")}&xhr=t");
                    postResult.EnsureSuccessStatusCode();

                    string suggestions;

                    using (var responseStream = await postResult.Content.ReadAsStreamAsync())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                suggestions = sr.ReadToEnd();
                            }
                        }
                    }

                    var suggestionList = GetCompletions_ParseSuggestions(suggestions);
                    return suggestionList;
                }
            }
        }

        private static string GetCompletions_GetValueOf(string document, string key)
        {
            char quote = '\"';
            string keyPattern = $"{quote}{key}{quote}:{quote}";

            int startPos_Value = document.IndexOf(keyPattern, 0) + keyPattern.Length;
            int endPos_Value = document.IndexOf(quote, startPos_Value + 1);

            string value = document.Substring(startPos_Value, endPos_Value - startPos_Value);
            return value;
        }

        private static List<string> GetCompletions_ParseSuggestions(string suggestions)
        {
            char quote = '\"';
            char bs = '\\';
            string beginSuggestion = $"[{quote}";
            string endSuggestion = $"{quote},";

            var results = new List<string>();
            int startPos = suggestions.IndexOf($"[[{quote}");

            if (startPos == -1)
            {
                startPos = suggestions.IndexOf($"[[{bs}{quote}");
                if (startPos == -1)
                {
                    return results;
                }
                else
                {
                    beginSuggestion = $"[{bs}{quote}";
                }
            }

            int endPos;
            while (true)
            {
                startPos = suggestions.IndexOf(beginSuggestion, startPos);
                if (startPos == -1)
                    break;
                endPos = suggestions.IndexOf(endSuggestion, startPos);
                startPos += beginSuggestion.Length;

                var suggestion = suggestions.Substring(startPos, endPos - startPos);
                suggestion = suggestion.Replace("\\u003cb\\u003e", string.Empty);
                suggestion = suggestion.Replace("\\u003c\\/b\\u003e", string.Empty);
                results.Add(suggestion);
            }

            return results;
        }

        private static string GetBestGuessForImage_ParsePostbackResult(string DOM)
        {
            string startDelimiter_1 = "Best guess for this image";
            string startDelimiter_2 = ">";
            string endDelimiter = "</a>";

            int idxStartDelimiter1 = DOM.IndexOf(startDelimiter_1);
            int idxStartDelimiter2 = DOM.IndexOf(startDelimiter_2, idxStartDelimiter1);
            int idxEndDelimiter = DOM.IndexOf(endDelimiter, idxStartDelimiter2);

            int idxStartResult = idxStartDelimiter2 + startDelimiter_2.Length;
            int resultLength = idxEndDelimiter - idxStartResult;
            string result = DOM.Substring(idxStartResult, resultLength);

            return result;
        }

        /*
         * Usage:
         *   string fullyQualifiedFileName = @"C:\Users\indra\Desktop\TEST Eiffel.jpg";
         *   string bestGuess = SearchUtils.GetBestGuessForImage(fullyQualifiedFileName).Result;
         */
        public static async Task<string> GetBestGuessForImage(string fullyQualifiedFileName)
        {
            string fileName = Path.GetFileName(fullyQualifiedFileName);
            byte[] byteArray = File.ReadAllBytes(fullyQualifiedFileName);
            return await GetBestGuessForImage(byteArray, fileName);
        }

        /*
         * Usage:
         *   string fullyQualifiedFileName = @"C:\Users\indra\Desktop\couple.jpg";
         *   string bestGuess = SearchUtils.GetBestGuessForImage(File.ReadAllBytes(fullyQualifiedFileName), Path.GetFileName(fullyQualifiedFileName)).Result;
         */
        public static async Task<string> GetBestGuessForImage(byte[] byteArray, string fileName = "photo.jpg")
        {
            var baseAddress = new Uri("https://images.google.com");
            //var cookieContainer = new CookieContainer();
            //using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(/*handler*/) { BaseAddress = baseAddress })
                {
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    client.DefaultRequestHeaders.Accept.Clear();
                    MediaTypeWithQualityHeaderValue media;
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("text/html;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("application/xhtml+xml;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("application/xml;q=0.9", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("image/webp;q=0.8", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);
                    media = null;
                    if (MediaTypeWithQualityHeaderValue.TryParse("*/*;q=0.8", out media))
                        client.DefaultRequestHeaders.Accept.Add(media);

                    client.DefaultRequestHeaders.AcceptEncoding.Clear();
                    client.DefaultRequestHeaders.AcceptEncoding.TryParseAdd("gzip, deflate, br");

                    client.DefaultRequestHeaders.AcceptLanguage.Clear();
                    client.DefaultRequestHeaders.AcceptLanguage.TryParseAdd("en-US,en;q=0.8,ca;q=0.6,de;q=0.4,fr;q=0.2,id;q=0.2,ms;q=0.2,nl;q=0.2,cs;q=0.2");

                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2926.0 Safari/537.36");

                    client.DefaultRequestHeaders.Referrer = new Uri("https://images.google.com/");
                    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    HttpResponseMessage homePageResult;
                    using (var form = new MultipartFormDataContentCompat())
                    {
                        using (var image_content = new StringContent(InternalHelper.ByteArrayToBase64(byteArray)))
                        {
                            using (var filename = new StringContent(fileName))
                            {
                                form.Add(image_content, "image_content");
                                form.Add(filename, "filename");
                                homePageResult = await client.PostAsync("/searchbyimage/upload", form);
                                homePageResult.EnsureSuccessStatusCode();
                            }
                        }
                    }

                    string DOM;
                    using (var responseStream = await homePageResult.Content.ReadAsStreamAsync())
                    {
                        using (var zippedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        {
                            using (var sr = new StreamReader(zippedStream))
                            {
                                DOM = sr.ReadToEnd();
                            }
                        }
                    }

                    //File.WriteAllText(@"C:\Users\indra\Desktop\DOM image.html", DOM);

                    var bestGuess = GetBestGuessForImage_ParsePostbackResult(DOM);
                    return bestGuess;
                }
            }
        }
    }
}