using System;
using System.IO;

namespace App2.Google.Search
{
    /*
     * http://stackoverflow.com/questions/13300510/unexpected-response-from-multipart-form-post-to-google/14046845#14046845
     */
    class InternalHelper
    {
        public static string FileToBase64(string imagePath)
        {
            byte[] content = File.ReadAllBytes(imagePath);
            return ByteArrayToBase64(content);
        }

        public static string ByteArrayToBase64(byte[] content)
        {
            string base64 = Convert.ToBase64String(content).Replace('+', '-').Replace('/', '_');
            return base64;
        }

        /*public static void UploadImage(string imagePath)
        {
            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContentCompat();
                form.Add(new StringContent(FileToBase64(imagePath)), "image_content");
                form.Add(new StringContent(Path.GetFileName(imagePath)), "filename");
                var response = client.PostAsync("https://images.google.com/searchbyimage/upload", form).Result;
                // Do whatever you want with the response
            }
        }*/
    }
}