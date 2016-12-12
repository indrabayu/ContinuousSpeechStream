using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    public class StanfordNLP
    {
        public static async Task<Tuple<string, List<List<NlpChunk>>>> Execute_HttpClient(string statement)
        {
            if (string.IsNullOrEmpty(statement))
                return new Tuple<string, List<List<NlpChunk>>>(string.Empty, null);

            string DOM = string.Empty;

            using (var httpClient = new HttpClient())
            {
                var mediaType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                var parameter = new NameValueHeaderValue("charset", "UTF-32");
                mediaType.Parameters.Add(parameter);

                var url = "http://nlp.stanford.edu:8080/parser/index.jsp";
                var uri = new Uri(url);

                var byteArray = Encoding.UTF8.GetBytes($"query={statement.Replace(" ", "+")}&btn=parseButton");
                using (MemoryStream ms = new MemoryStream(byteArray, 0, byteArray.Length))
                {
                    var param = new StreamContent(ms);
                    param.Headers.ContentType = mediaType;

                    var result = await httpClient.PostAsync(uri, param);

                    DOM = await result.Content.ReadAsStringAsync();
                }
            }

            //File.WriteAllText(@"C:\Users\indra\Desktop\StanfordOnline_Full.txt.html", DOM);

            /////////////////////////////////////////////////////////////////////////////////////

            var nlpPackage = GetNlpStatements(DOM);

            //File.WriteAllText(@"C:\Users\indra\Desktop\StanfordOnline_Answer.txt.html", nlpPackage.Item1);

            //string toPrint = string.Empty;

            //toPrint = nlpPackage.Item1;

            //foreach (var nlpStatement in nlpPackage.Item2)
            //{
            //    foreach (var nlpChunk in nlpStatement)
            //    {
            //        toPrint += nlpChunk.Part + " ";
            //    }
            //    toPrint += Environment.NewLine;
            //}

            //toPrint = toPrint.Trim().Replace(" .", ".");

            //Console.WriteLine(toPrint);

            return nlpPackage;
        }

        /*
         *  taken from: http://stackoverflow.com/questions/10583794/httpwebrequest-virtual-button-click 
         */
        public static Tuple<string, List<List<NlpChunk>>> Execute_WebRequest(string statement)
        {
            const string target = "http://nlp.stanford.edu:8080/parser/index.jsp";
            string DOM = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(target);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-32";
            using (var stream = request.GetRequestStream())
            {
                var buffer = Encoding.UTF8.GetBytes($"query={statement.Replace(" ", "+")}&btn=parseButton");
                stream.Write(buffer, 0, buffer.Length);
            }
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                DOM = reader.ReadToEnd();
            }
            //File.WriteAllText(@"C:\Users\indra\Desktop\StanfordOnline_Full.txt.html", DOM);

            /////////////////////////////////////////////////////////////////////////////////////

            var nlpPackage = GetNlpStatements(DOM);

            //File.WriteAllText(@"C:\Users\indra\Desktop\StanfordOnline_Answer.txt.html", nlpPackage.Item1);

            //Console.WriteLine(nlpPackage.Item1);

            //foreach (var nlpStatement in nlpPackage.Item2)
            //{
            //    foreach (var nlpChunk in nlpStatement)
            //    {
            //        Console.Write(nlpChunk.Part + " ");
            //    }
            //    Console.WriteLine();
            //}

            return nlpPackage;
        }

        private static Tuple<string, List<List<NlpChunk>>> GetNlpStatements(string DOM)
        {
            List<List<NlpChunk>> nlp_statements = new List<List<NlpChunk>>();

            string _strBegin = "<div class=\"parserOutputMonospace\">";
            string _strEnd = "<div style=\"clear: left\">";

            int begin = DOM.IndexOf(_strBegin);
            int end = DOM.IndexOf(_strEnd);

            string raw_nlp_paragraph = DOM.Substring(begin + _strBegin.Length, end - begin - _strEnd.Length - _strBegin.Length).Trim();
            raw_nlp_paragraph = raw_nlp_paragraph.Replace("<div style=\"padding-right: 1em; float: left; white-space: nowrap;\">", "").Trim();
            raw_nlp_paragraph = raw_nlp_paragraph.Replace("\n", "").Trim();
            var parts = raw_nlp_paragraph.Split(new string[] { "</div>" }, StringSplitOptions.RemoveEmptyEntries);
            raw_nlp_paragraph = "";

            var nlp_statement = new List<NlpChunk>();
            foreach (var item in parts)
            {
                string chunk = item.Trim();
                chunk = chunk.Replace(" ", "");
                if (!string.IsNullOrEmpty(chunk) && !string.IsNullOrWhiteSpace(chunk) && chunk.Contains('/'))
                {
                    if (chunk.StartsWith("<br/>"))
                    {
                        nlp_statements.Add(nlp_statement);
                        nlp_statement = new List<NlpChunk>();
                        chunk = chunk.Replace("<br/>", "");
                        raw_nlp_paragraph += Environment.NewLine;
                    }

                    raw_nlp_paragraph += $"[{chunk}] ";
                    nlp_statement.Add(new NlpChunk(chunk));
                }
            }
            if (nlp_statement.Count > 0)
            {
                nlp_statements.Add(nlp_statement);
            }

            return new Tuple<string, List<List<NlpChunk>>>(raw_nlp_paragraph, nlp_statements);
        }
    }

    public class NlpChunk
    {
        public NlpChunk(string chunk)
        {
            var _ = chunk.Split('/');
            Part = _[0];
            Tag = _[1];
        }

        public string Part { get; set; }
        public string Tag { get; set; }
    }
}
