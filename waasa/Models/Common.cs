using System.Windows.Documents;
using waasa.Services;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace waasa.Models {

    public enum CfStatus {
        Unknown,
        Blocked,
        Bypassed,
        Allowed
    }

    public enum Judgement {
        Unknown,
        Good,
        Bad,
        Careful
    }

    public class _FileExtension {
        public string Extension { get; set; } = ""; // reference, e.g. ".exe"
        public string Result { get; set; } = "";  // how the extension will be opened
        public string Assumption { get; set; } = "";  //
        public Judgement Judgement { get; set; } = Judgement.Unknown; // how dangerous it is

        // All from WinApi and Registry
        public string AppName { get; set; } = ""; 
        public string AppPublisher { get; set; } = "";
        public string CmdLine { get; set; } = "";
        public string CmdExePath { get; set; } = "";
        public string CmdArgs { get; set; } = "";
        public bool isUwp { get; set; } = false;

        // Loaded from external reference file
        public InfoExtension InfoExtension { get; set; } = new InfoExtension();
        
        public string Description { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();

        // Pointer to source
        public Winapi.WinapiEntry WinApiEntry { get; set; } = new Winapi.WinapiEntry();

        // Content Filter Results
        public List<TestResult> TestResults { get; set; } = new List<TestResult> {
            new TestResult(),
            new TestResult(),
            new TestResult()
        };

        public string TestResult { get; set; } = "";


        public _FileExtension(string extension) {
            Extension = extension.ToLower();
        }

        public void SetCmd(string cmd) {
            this.CmdLine = cmd;
            var res = CmdParser.CommandLineToResult(cmd);
            this.CmdExePath = res.Item1;
            this.CmdArgs = res.Item2;
        }
    }


    class _CsvEntry {
        public string Extension { get; set; } = "";
        public string Assumption { get; set; } = "";
        public Judgement Judgement { get; set; } = Judgement.Unknown;
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
            AppPath = fileExtension.CmdLine;
        }
    }

}
