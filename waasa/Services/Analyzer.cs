using System;
using System.Collections.Generic;
using waasa.Models;
using Serilog;


namespace waasa.Services {

    /// <summary>
    /// Merges GatheredData (and its simpleView) with Validator to produce FileExtensions 
    /// which contain all extensions and their associated path.
    /// </summary>
    public class Analyzer {
        private _GatheredData GatheredData { get; set; } = new _GatheredData();
        private GatheredDataSimpleView SimpleDataView { get; set; } = new GatheredDataSimpleView(new _GatheredData());
        private List<_FileExtension> FileExtensions { get; set; } = new List<_FileExtension>();


        public Analyzer() {
        }


        public void Load(_GatheredData gatheredData, Validator validator, GatheredDataSimpleView registry) {
            Log.Information("Analyzer: Load and resolve extensions");

            GatheredData = gatheredData;
            SimpleDataView = registry;

            foreach (var extension in GatheredData.ListedExtensions) {
                var fileExtension = resolveExtension(extension);
                FileExtensions.Add(fileExtension);
            }
        }


        public List<_FileExtension> getResolvedFileExtensions() {
            return FileExtensions;
        }


        private _FileExtension resolveExtension(string extension) {
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Extension = extension;

            // Data
            var winapiData = GatheredData.WinapiData[extension];
            fileExtension.AppName = winapiData.FriendlyAppName;
            fileExtension.WinApiEntry = winapiData;

            // Analyze data to clean up and add all information for this extension
            fileExtension.SetCmd(winapiData.Command);
            analyzeExtension(fileExtension);

            return fileExtension;
        }


        // This implements the main algorithm to categorize file extension
        private void analyzeExtension(_FileExtension fileExtension) {
            // Logic to decide the assumption (how the file will be opened)
            var assumption = "";
            if (fileExtension.WinApiEntry.FriendlyAppName.StartsWith("Pick an app")) {
                assumption = "openwith1";

            } else if (fileExtension.WinApiEntry.FriendlyAppName == "") {
                if (fileExtension.WinApiEntry.Command != "" 
                    && !SimpleDataView.isValidRootProgids(fileExtension.Extension) 
                    && SimpleDataView.hasRootDefault(fileExtension.Extension)) 
                {
                    // May also use: Root_DefaultExec
                    // Basically just .cmd, .com
                    assumption = "exec1";
                } else {
                    assumption = "openwith2";
                }

            } else if (fileExtension.WinApiEntry.Command != "") {
                assumption = "exec2";
            } else {
                if (SimpleDataView.countUserOpenWithProgids(fileExtension.Extension) < 2) {
                    assumption = "exec3";
                } else {
                    assumption = "recommended1";
                }
            }
            fileExtension.Assumption = assumption;

            // get real destination path
            var appPath = fileExtension.AppPath;
            if (appPath == "") {
                // Attempt to resolve from MS Store packages
                if (UwpAnalyzer.progidToPath(fileExtension.WinApiEntry.Progid, GatheredData) != null) {
                    appPath = UwpAnalyzer.progidToPath(fileExtension.WinApiEntry.Progid, GatheredData);
                    fileExtension.isUwp = true;
                    var exe = UwpAnalyzer.parseManifest(appPath + "\\" + "AppxManifest.xml");
                    appPath += "\\" + exe;
                    fileExtension.SetCmd('\"' + appPath + '\"');
                }

                // ???
                if (appPath == "") {
                    // Content-Type -> Media player related
                    if (SimpleDataView.getRootContentType(fileExtension.Extension) != "") {
                        var exec = SimpleDataView.ContentTypeExec(SimpleDataView.getRootContentType(fileExtension.Extension));
                        Console.WriteLine("A1: " + fileExtension.Extension + " " + exec);
                        appPath = exec;
                    }
                }

                if (appPath == "") {
                    // Windows SystemApps related (Userchoice)
                    if (SimpleDataView.getUserChoice(fileExtension.Extension) != "") {
                        var exec = SimpleDataView.GetSystemApp(SimpleDataView.getUserChoice(fileExtension.Extension));
                        Console.WriteLine("A2: " + fileExtension.Extension + " " + exec);
                        appPath += exec;
                    }

                }
                if (appPath == "") {
                    // Windows SystemApps related ()
                    if (SimpleDataView.countRootProgids(fileExtension.Extension) == 1) {
                        var exec = SimpleDataView.GetSystemApp(SimpleDataView.getRootProgid(fileExtension.Extension));
                        Console.WriteLine("A3: " + fileExtension.Extension + " " + exec);
                        appPath += exec;
                    }
                }

                fileExtension.AppPath = appPath;
            }
        }
    }
}