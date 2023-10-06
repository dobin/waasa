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
        public string AppPublisher { get; set; } = "";

        // Pointer to source
        public Winapi.WinapiEntry WinApiEntry { get; set; } = new Winapi.WinapiEntry();

        // Results
        public List<TestResult> TestResults { get; set; } = new List<TestResult> {
            new TestResult(),
            new TestResult(),
            new TestResult()
        };
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
