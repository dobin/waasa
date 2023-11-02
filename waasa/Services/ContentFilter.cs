using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using waasa.Models;
using Serilog;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

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
            foreach (var ext in fileExtension) {
                await analyzeExtension(ext);
            }
        }

        public static async Task<string> CheckContentFilter() {
            Log.Information("Content Filter check");
            string server = Properties.Settings.Default.WAASA_SERVER;
            HttpAnswerInfo answer;
            string ret = "";

            answer = await Requestor.Get("test.txt", server, "standard");
            if (answer.HashCheck) {
                ret += "Content Filter check standard: Bypass" + "\n";
            }

            answer = await Requestor.Get("test.txt", server, "nomime");
            if (answer.HashCheck) {
                ret += "Content Filter check no-mime: Bypass" + "\n";
            }

            answer = await Requestor.Get("test.txt", server, "nomimenofilename");
            if (answer.HashCheck) {
                ret += "Content Filter check no-mime no-filename: Bypass" + "\n";
            }
            Log.Information("Content Filter check: " + ret + "\n");
            return ret;
        }


        public static async Task analyzeExtension(_FileExtension fileExtension) {
            string server = Properties.Settings.Default.WAASA_SERVER;
            string ext = fileExtension.Extension;
            string api;
            HttpAnswerInfo answer;
            string res = "";

            Log.Information("Content Filter test for: " + fileExtension.Extension + " on server " + server);

            try {
                api = "standard";
                answer = await Requestor.Get("test" + ext, server, api);
                fileExtension.TestResults[0].Api = api;
                fileExtension.TestResults[0].HttpAnswerInfo = answer;
                if (! answer.HashCheck) {
                    fileExtension.TestResults[0].Conclusion = "blocked";
                    res += "block/";
                } else {
                    fileExtension.TestResults[0].Conclusion = "bypass";
                    res += "bypass/";
                }

                api = "nomime";
                answer = await Requestor.Get("test" + ext, server, api);
                fileExtension.TestResults[1].Api = api;
                fileExtension.TestResults[1].HttpAnswerInfo = answer;
                if (! answer.HashCheck) {
                    fileExtension.TestResults[1].Conclusion = "blocked";
                    res += "block/";
                } else {
                    fileExtension.TestResults[1].Conclusion = "bypass";
                    res += "bypass/";
                }

                api = "nomimenofilename";
                answer = await Requestor.Get("test" + ext, server, api);
                fileExtension.TestResults[2].Api = api;
                fileExtension.TestResults[2].HttpAnswerInfo = answer;
                if (! answer.HashCheck) {
                    fileExtension.TestResults[2].Conclusion = "blocked";
                    res += "block ";
                } else {
                    fileExtension.TestResults[2].Conclusion = "bypass";
                    res += "bypass ";
                }

            } catch (Exception e) {
                res = "error";
                MessageBox.Show("Error while analyzing extension: " + e.Message.ToString());
            }   

            fileExtension.TestResult = res;
        }
    }
}
