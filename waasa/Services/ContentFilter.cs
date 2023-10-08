using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using waasa.Models;


namespace waasa.Services {

    public class TestResult {
        public string Api { get; set; } = "";
        public int HttpStatusCode { get; set; } = 0;
        public string HttpAnswer { get; set; } = "";
        public string Conclusion { get; set; } = "";

        public TestResult() {
        }
    }


    public class ContentFilter {
        public ContentFilter() { }

        public static async Task analyzeExtension(_FileExtension fileExtension) {
            string server = Properties.Settings.Default.WAASA_SERVER;
            string ext = fileExtension.Extension;
            string api;
            HttpAnswerInfo answer;

            api = "standard";
            answer = await Requestor.Get("test" + ext, server, api);
            fileExtension.TestResults[0].Api = api;
            fileExtension.TestResults[0].HttpStatusCode = answer.StatusCode;
            fileExtension.TestResults[0].HttpAnswer = answer.Content;
            if (answer.Content != "data") {
                fileExtension.TestResults[0].Conclusion = "blocked";
            } else {
                fileExtension.TestResults[0].Conclusion = "bypass";
            }

            api = "nomime";
            answer = await Requestor.Get("test" + ext, server, api);
            fileExtension.TestResults[1].Api = api;
            fileExtension.TestResults[1].HttpStatusCode = answer.StatusCode;
            fileExtension.TestResults[1].HttpAnswer = answer.Content;
            if (answer.Content != "data") {
                fileExtension.TestResults[1].Conclusion = "blocked";
            } else {
                fileExtension.TestResults[1].Conclusion = "bypass";
            }

            api = "nomimenofilename";
            answer = await Requestor.Get("test" + ext, server, api);
            fileExtension.TestResults[2].Api = api;
            fileExtension.TestResults[2].HttpStatusCode = answer.StatusCode;
            fileExtension.TestResults[2].HttpAnswer = answer.Content;
            if (answer.Content != "data") {
                fileExtension.TestResults[2].Conclusion = "blocked";
            } else {
                fileExtension.TestResults[2].Conclusion = "bypass";
            }
        }
    }
}
