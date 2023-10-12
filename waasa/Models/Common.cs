using System.Windows.Documents;
using waasa.Services;
using System.Collections.Generic;


namespace waasa.Models {

    public class _FileExtension {
        public string Extension { get; set; } = "";
        public string Result { get; set; } = "";
        public string Assumption { get; set; } = "";
        public string Judgement { get; set; } = "";

        public string AppName { get; set; } = "";
        public string AppPath { get; set; } = "";
        public string CmdName { get; set; } = "";
        public string CmdArgs { get; set; } = "";
        public string AppPublisher { get; set; } = "";

        public bool isUwp { get; set; } = false;

        public _FileExtension(string extension) {
            Extension = extension;
        }

        // Pointer to source
        public Winapi.WinapiEntry WinApiEntry { get; set; } = new Winapi.WinapiEntry();

        // Results
        public List<TestResult> TestResults { get; set; } = new List<TestResult> {
            new TestResult(),
            new TestResult(),
            new TestResult()
        };
        public string TestResult { get; set; } = "";

        public void SetCmd(string cmd) {
            this.AppPath = cmd;
            var res = CmdParser.CommandLineToResult(cmd);
            this.CmdName = res.Item1;
            this.CmdArgs = res.Item2;
        }
    }


    class _CsvEntry {
        public string Extension { get; set; } = "";
        public string Assumption { get; set; } = "";
        public string Judgement { get; set; } = "";
        public string AppName { get; set; } = "";
        public string AppPath { get; set; } = "";


        public _CsvEntry(_FileExtension fileExtension) {
            Extension = fileExtension.Extension;

            Assumption = fileExtension.Assumption;
            if (fileExtension.Assumption.StartsWith("exec")) {
                Assumption = "program execution";
            }
            if (fileExtension.Assumption.StartsWith("openwith")) {
                Assumption = "open-with list";
            }
            if (fileExtension.Assumption.StartsWith("recommended")) {
                Assumption = "recommended list";
            }

            Judgement = fileExtension.Judgement;
            AppName = fileExtension.AppName;
            AppPath = fileExtension.AppPath;
        }
    }

}
