using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace waasa
{
	public class _FileExtension
	{
		public string Extension { get; set; }
        public string Result { get; set; }
		public string Assumption { get; set; }

        public string Judgement { get; set; }

        public string AppName { get; set; }
        public string AppPath { get; set; }
    }


    public class Analyzer
    {
        public _GatheredData GatheredData { get; set; }
        public Validator Validator { get; set; }
        private VirtRegistry Registry { get; set; }


        public Analyzer(_GatheredData gatheredData, Validator validator, VirtRegistry registry)
        {
            GatheredData = gatheredData;
            Validator = validator;
            Registry = registry;
        }


        // Analyze all GatheredData to produce FileExtensions
        public List<_FileExtension> AnalyzeGatheredData()
        {
            List<_FileExtension> fileExtensions = new List<_FileExtension>();
            foreach (var extension in GatheredData.ListedExtensions) {
                var fileExtension = AnalyzeSingle(extension);
                fileExtensions.Add(fileExtension);
            }
            return fileExtensions;
        }


        public _FileExtension AnalyzeSingle(string extension)
        {
            var data = AnalyzeExtension(extension);
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Result = Validator.GetEffectiveResultFor(extension);
            fileExtension.Extension = extension;
            fileExtension.Assumption = data.Item1;
            fileExtension.AppName = data.Item2;
            fileExtension.AppPath = data.Item3;
            return fileExtension;
        }


        public Shlwapi.Assoc GetShlwapiBy(string ext)
        {
            if (GatheredData.ShlwapiAssoc.ContainsKey(ext)) {
                return GatheredData.ShlwapiAssoc[ext];
            } else {
                return null;
            }
        }


        // This implements the main algorithm to categorize file extension
        public Tuple<string, string, string> AnalyzeExtension(string extension)
        {
            var assoc = GetShlwapiBy(extension);

            string assumption = "";
            string appName = assoc.FriendlyAppName;
            string appPath = assoc.Command;

            if (assoc.FriendlyAppName == "Pick an application") {
                assumption = "openwith1";

            } else if (assoc.FriendlyAppName == "") {
                if (assoc.Command != "" && !Registry.isValidRootProgids(extension)) {
                    // May also use: Root_DefaultExec
                    // Basically just .cmd, .com
                    assumption = "exec2";
                } else { 
                    assumption = "openwith2";
                }
                
            } else if (assoc.Command != "") {
                // Hmmm
                if (! Registry.isValidUserProgids(extension) && Registry.countUserOpenWithList(extension) == "0") {
                    assumption = "openwith3";
                } else {
                    assumption = "exec3";
                }
            } else {
                if (Registry.countUserOpenWithProgids(extension) < 2) {
                    assumption = "exec4";
                } else {
                    assumption = "recommended4";
                }
            }

            // get real destination
            if (appPath == "") {
                // Content-Type -> Media player related
                if (Registry.getRootContentType(extension) != "") {
                    var exec = Registry.ContentTypeExec(Registry.getRootContentType(extension));
                    appPath = exec;
                }

                if (appPath == "") {
                    // Windows SystemApps related (Userchoice)
                    if (Registry.getUserChoice(extension) != "") {
                        var exec = Registry.GetSystemApp(Registry.getUserChoice(extension));
                        appPath += exec;
                    }

                }
                if (appPath == "") {
                    // Windows SystemApps related ()
                    if (Registry.countRootProgids(extension) == 1) {
                        var exec = Registry.GetSystemApp(Registry.getRootProgids(extension));
                        appPath += exec;
                    }
                }
            }

            return new Tuple<string, string, string>(assumption, appName, appPath);
        }
    }
}