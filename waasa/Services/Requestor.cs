using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;


namespace waasa.Services {

    public class HttpAnswerInfo {
        
        public string Filename { get; set; } = "";
        public bool HashCheck { get; set; } = false;

        //public string InitialFilename { get; set; } = "";
        //public bool IsSuccess { get; set; }
        public bool IsRealFile { get; set; } = false;

        public string HttpRequest { get; set; } = "";
        public string HttpResponse { get; set; } = "";
        public int HttpResponseStatusCode { get; set; } = 0;

        public HttpAnswerInfo(string httpRequest, string httpResponse, int statusCode, string filename, bool hashCheck, bool isRealFile) {
            HttpRequest = httpRequest;
            HttpResponse = httpResponse;
            HttpResponseStatusCode = statusCode;
            Filename = filename;
            HashCheck = hashCheck;
            IsRealFile = isRealFile;
        }
    }

    class Requestor {
        private static readonly HttpClient client = new HttpClient();


        public static async Task<HttpAnswerInfo> Get(string filename, string server = "http://localhost:5002", string api = "simple") {
            /*var uriBuilder = new UriBuilder {
                Scheme = "http",
                Host = "www.example.com",
                Path = "path/to/resource",
                Query = "param1=value1&param2=value2"
            };*/

            if (api == "nomimenofilename") {
                filename = Convert.ToBase64String(Encoding.UTF8.GetBytes(filename));
            }

            string url = $"{server}/{api}/{filename}";
            Log.Information("HTTP Request: " + url);

            string requestContent;
            string responseContent;

            HttpResponseMessage response;

            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url)) {
                //var requestContent = request.ToString();
                requestContent = await HttpClientExtensions.ToRawString(request);

                using (response = await httpClient.SendAsync(request)) {
                    // Process the response
                    var responseData = response.StatusCode.ToString();
                    responseContent = await HttpClientExtensions.ToRawString(response);

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
                    string? referenceHash = null;
                    if (response.Headers.TryGetValues("X-Hash", out var values)) {
                        referenceHash = values.First();
                    }

                    bool hashCheck = false;
                    byte[] contentBytes = await response.Content.ReadAsByteArrayAsync();
                    using (SHA256 sha256 = SHA256.Create()) {
                        byte[] hashBytes = sha256.ComputeHash(contentBytes);
                        string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                        if (referenceHash != hashString) {
                            hashCheck = false;
                        } else {
                            hashCheck = true;
                        }
                    }

                    bool isRealFile = false;
                    if (responseData != "data") {
                        isRealFile = true;
                    }

                    HttpAnswerInfo answer = new HttpAnswerInfo(
                        requestContent, responseContent,
                        statuscode, recvFilename, hashCheck, isRealFile);
                    return answer;
                }
            }
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


    // Copied from https://www.jordanbrown.dev/2021/02/06/2021/http-to-raw-string-csharp/
    public static class HttpClientExtensions {
        public static async Task<string> ToRawString(this HttpRequestMessage request) {
            var sb = new StringBuilder();

            var line1 = $"{request.Method} {request.RequestUri} HTTP/{request.Version}";
            sb.AppendLine(line1);

            foreach (var (key, value) in request.Headers)
                foreach (var val in value) {
                    var header = $"{key}: {val}";
                    sb.AppendLine(header);
                }

            if (request.Content?.Headers != null) {
                foreach (var (key, value) in request.Content.Headers)
                    foreach (var val in value) {
                        var header = $"{key}: {val}";
                        sb.AppendLine(header);
                    }
            }
            sb.AppendLine();

            var body = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult<string>(null));
            if (!string.IsNullOrWhiteSpace(body))
                sb.AppendLine(body);

            return sb.ToString();
        }

        public static async Task<string> ToRawString(this HttpResponseMessage response) {
            var sb = new StringBuilder();

            var statusCode = (int)response.StatusCode;
            var line1 = $"HTTP/{response.Version} {statusCode} {response.ReasonPhrase}";
            sb.AppendLine(line1);

            foreach (var (key, value) in response.Headers)
                foreach (var val in value) {
                    var header = $"{key}: {val}";
                    sb.AppendLine(header);
                }

            foreach (var (key, value) in response.Content.Headers)
                foreach (var val in value) {
                    var header = $"{key}: {val}";
                    sb.AppendLine(header);
                }
            sb.AppendLine();

            var body = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(body)) {
                if (body.Length < 8) {
                    sb.AppendLine(body);
                } else {
                    sb.AppendLine(body.Substring(0, 8) + "...");
                }
            }

            return sb.ToString();
        }
    }
}
