using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using waasa.Models;
using Serilog;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Http;

namespace waasa.Services {

    public class TestResult {
        public string Api { get; set; } = "";
        public string HttpAnswer { get; set; } = "";
        public HttpAnswerInfo HttpAnswerInfo { get; set; } = new HttpAnswerInfo("", "", 0, "", false, false);
        public string Conclusion { get; set; } = "";

        public TestResult() {
        }
    }


    public class ContentFilter {
        public ContentFilter() { }


        public static async Task analyzeExtensions(List<_FileExtension> fileExtension) {
            var tasks = fileExtension.Select(fe => analyzeExtension(fe));
            await Task.WhenAll(tasks);
        }


        private static async Task downloadCf(_FileExtension fileExtension, string api, string server) {
            HttpAnswerInfo answer = await Requestor.Get("test" + fileExtension.Extension, server, api);

            int idx = -1;
            if (api == "standard") {
                idx = 0;
            } else if (api == "nomime") {
                idx = 1;
            } else if (api == "nomimenofilename") {
                idx = 2;
            }

            fileExtension.TestResults[idx].Api = api;
            fileExtension.TestResults[idx].HttpAnswerInfo = answer;
            if (!answer.HashCheck) {
                fileExtension.TestResults[idx].Conclusion = "blocked";
            } else {
                fileExtension.TestResults[idx].Conclusion = "bypass";
            }
        }


        public static async Task analyzeExtension(_FileExtension fileExtension) {
            string server = Properties.Settings.Default.WAASA_SERVER;
            Log.Information("Content Filter test for: " + fileExtension.Extension + " on server " + server);

            // Download all in parallel and wait for completion
            List<string> batch = new List<string>() { "standard", "nomime", "nomimenofilename" };
            try {
                var tasks = batch.Select(api => downloadCf(fileExtension, api, server));
                await Task.WhenAll(tasks);
            } catch (Exception e) {
                MessageBox.Show("Error while analyzing extension: " + e.Message.ToString());
            }

            string summary = "";
            foreach(var res in fileExtension.TestResults) {
                if (res.Conclusion == "blocked") {
                    summary += "blocked ";
                } else if (res.Conclusion == "bypass") {
                    summary += "bypass ";
                }
            }
            fileExtension.TestResult = summary;
        }
    }
}
