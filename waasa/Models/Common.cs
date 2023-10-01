using waasa.Services;


namespace waasa.Models {

    public class _FileExtension {
        public string Extension { get; set; } = "";
        public string Result { get; set; } = "";
        public string Assumption { get; set; } = "";
        public string Judgement { get; set; } = "";

        public string AppName { get; set; } = "";
        public string AppPath { get; set; } = "";
        public string AppPublisher { get; set; } = "";

        // Needed?
        public Winapi.WinapiEntry WinApiEntry { get; set; }
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
