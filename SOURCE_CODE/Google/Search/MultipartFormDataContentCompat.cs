using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace App2.Google.Search
{
    public class MultipartFormDataContentCompat : MultipartContent
    {
        public MultipartFormDataContentCompat() : base("form-data")
        {
            FixBoundaryParameter();
        }

        public MultipartFormDataContentCompat(string boundary) : base("form-data", boundary)
        {
            FixBoundaryParameter();
        }

        public override void Add(HttpContent content)
        {
            base.Add(content);
            AddContentDisposition(content, null, null);
        }

        public void Add(HttpContent content, string name)
        {
            base.Add(content);
            AddContentDisposition(content, name, null);
        }

        public void Add(HttpContent content, string name, string fileName)
        {
            base.Add(content);
            AddContentDisposition(content, name, fileName);
        }

        private void AddContentDisposition(HttpContent content, string name, string fileName)
        {
            var headers = content.Headers;
            if (headers.ContentDisposition != null)
                return;
            headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = QuoteString(name),
                FileName = QuoteString(fileName)
            };
        }

        private string QuoteString(string str)
        {
            return $"\"{str}\""; //'"' + str + '"';
        }

        private void FixBoundaryParameter()
        {
            var boundary = Headers.ContentType.Parameters.Single(p => p.Name.Equals("boundary"));
            boundary.Value = boundary.Value.Trim('"');
        }
    }
}