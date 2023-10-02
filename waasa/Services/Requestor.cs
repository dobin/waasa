using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;


namespace waasa.Services {
    class HttpAnswerInfo {
        public int StatusCode { get; set; } = 0;
        public string Filename { get; set; } = "";
        public string Content { get; set; } = "";

        public string InitialFilename { get; set; } = "";
        public Boolean IsSuccess { get; set; }


        public HttpAnswerInfo(int statusCode, string filename, string content) {
            StatusCode = statusCode;
            Filename = filename;
            Content = content;
        }
    }

    class Requestor {
        private static readonly HttpClient client = new HttpClient();


        public static async Task<HttpAnswerInfo> Get(string filename) {
            Console.WriteLine("Requestor: " + filename);

            string url = "http://localhost:5002/" + "simple/" + filename;
            HttpResponseMessage response = await client.GetAsync(url);
            string responseData = await response.Content.ReadAsStringAsync();

            int statuscode = (int)response.StatusCode;
            string recvFilename = "";
            if (response.Content.Headers.ContentDisposition != null) {
                string contentDisposition = response.Content.Headers.ContentDisposition.ToString();
                string? fn = GetFileNameFromContentDisposition(contentDisposition);
                if (fn == null) { 
                    // Didnt work
                } else {
                    recvFilename = fn;
                }
            }

            HttpAnswerInfo answer = new HttpAnswerInfo(statuscode, recvFilename, responseData);
            return answer;

        }

        public static string? GetFileNameFromContentDisposition(string contentDisposition) {
            const string filenameToken = "filename=";
            int startIndex = contentDisposition.IndexOf(filenameToken, StringComparison.OrdinalIgnoreCase);
            if (startIndex > -1) {
                string filename = contentDisposition.Substring(startIndex + filenameToken.Length);
                if (filename.StartsWith("\"") && filename.EndsWith("\"")) {
                    filename = filename.Trim('"');
                }
                return filename;
            }
            return null;
        }
    }
}
