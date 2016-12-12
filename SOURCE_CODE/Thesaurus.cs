using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace App2
{
    public class Thesaurus
    {
        public static async Task<List<string>> GetSynonims(string word)
        {
            var baseAddress = new Uri($"http://www.thesaurus.com/browse/{word}");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            {
                using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
                {
                    //usually i make a standard request without authentication, eg: to the home page.
                    //by doing this request you store some initial cookie values, that might be used in the subsequent login request and checked by the server
                    var response = await client.GetAsync("");
                    //response.EnsureSuccessStatusCode();

                    string DOM = await response.Content.ReadAsStringAsync();
                    if (DOM.Contains("no thesaurus results"))
                    {
                        const string didYouMean = "Did you mean";
                        if (DOM.Contains(didYouMean))
                        {
                            const string recursive_beginDelimiter = "<a href=\"http://www.thesaurus.com/browse/";
                            const string recursive_endDelimiter = "\">";
                            int recursive_beginDelimiterLength = recursive_beginDelimiter.Length;
                            int recursive_endDelimiterLength = recursive_endDelimiter.Length;

                            int curr_recursive_beginDelimiter_pos = DOM.IndexOf(recursive_beginDelimiter, DOM.IndexOf(didYouMean) + didYouMean.Length);
                            if (curr_recursive_beginDelimiter_pos < 0)
                                return null;

                            int curr_recursive_endDelimiter_pos = DOM.IndexOf(recursive_endDelimiter, curr_recursive_beginDelimiter_pos);
                            if (curr_recursive_endDelimiter_pos < 0)
                                return null;

                            int startIndex_suggestion = curr_recursive_beginDelimiter_pos + recursive_beginDelimiterLength;
                            int length_suggestion = curr_recursive_endDelimiter_pos - curr_recursive_beginDelimiter_pos - recursive_beginDelimiterLength;
                            string suggestion = DOM.Substring(startIndex_suggestion, length_suggestion);

                            return await GetSynonims(suggestion);
                        }
                        else
                            return null;
                    }


                    var synonims = new List<string>();

                    const string beginDelimiter = "<span class=\"text\">";
                    const string endDelimiter = "</span>";
                    int beginDelimiterLength = beginDelimiter.Length;
                    int endDelimiterLength = endDelimiter.Length;

                    int curr_beginDelimiter_pos = 0;
                    int curr_endDelimiter_pos = 0;
                    while (true)
                    {
                        curr_beginDelimiter_pos = DOM.IndexOf(beginDelimiter, curr_beginDelimiter_pos);
                        if (curr_beginDelimiter_pos < 0)
                            break;

                        int startIndex = curr_beginDelimiter_pos + beginDelimiterLength;
                        curr_endDelimiter_pos = startIndex;

                        curr_endDelimiter_pos = DOM.IndexOf(endDelimiter, curr_endDelimiter_pos);
                        if (curr_endDelimiter_pos < 0)
                            break;

                        int length = curr_endDelimiter_pos - curr_beginDelimiter_pos - beginDelimiterLength;

                        string synonim = DOM.Substring(startIndex, length);

                        synonims.Add(synonim);

                        curr_beginDelimiter_pos = curr_endDelimiter_pos = curr_endDelimiter_pos + endDelimiterLength;
                    }

                    return synonims;
                }
            }
        }
    }
}