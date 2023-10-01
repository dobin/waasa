using System;
using System.Collections.Generic;
using waasa.Models;
using static System.Net.Mime.MediaTypeNames;


namespace waasa.Services {

    public class Analyzer {
        public _GatheredData GatheredData { get; set; }
        public Validator Validator { get; set; }
        private GatheredDataSimpleView Registry { get; set; }


        public Analyzer() {
        }


        public void Load(_GatheredData gatheredData, Validator validator, GatheredDataSimpleView registry) {
            GatheredData = gatheredData;
            Validator = validator;
            Registry = registry;
        }


        // Analyze all GatheredData to produce FileExtensions
        public List<_FileExtension> AnalyzeGatheredData() {
            List<_FileExtension> fileExtensions = new List<_FileExtension>();
            foreach (var extension in GatheredData.ListedExtensions) {
                var fileExtension = HandleExtension(extension);
                fileExtensions.Add(fileExtension);
            }
            return fileExtensions;
        }


        public _FileExtension HandleExtension(string extension) {
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Extension = extension;

            // Validator Result
            fileExtension.Result = Validator.GetEffectiveResultFor(extension);

            // Data
            var winapiData = GatheredData.WinapiData[extension];
            fileExtension.AppName = winapiData.FriendlyAppName;
            fileExtension.AppPath = winapiData.Command;
            fileExtension.WinApiEntry = winapiData;

            // Analyze data to clean up and add all information for this extension
            AnalyzeExtension(fileExtension);

            return fileExtension;
        }


        // This implements the main algorithm to categorize file extension
        public void AnalyzeExtension(_FileExtension fileExtension) {
            var assumption = "";
            if (fileExtension.WinApiEntry.FriendlyAppName.StartsWith("Pick an app")) {
                assumption = "openwith1";

            } else if (fileExtension.WinApiEntry.FriendlyAppName == "") {
                if (fileExtension.WinApiEntry.Command != "" 
                    && !Registry.isValidRootProgids(fileExtension.Extension) 
                    && Registry.hasRootDefault(fileExtension.Extension)) 
                {
                    // May also use: Root_DefaultExec
                    // Basically just .cmd, .com
                    assumption = "exec2";
                } else {
                    assumption = "openwith2";
                }

            } else if (fileExtension.WinApiEntry.Command != "") {
                assumption = "exec3";
            } else {
                if (Registry.countUserOpenWithProgids(fileExtension.Extension) < 2) {
                    assumption = "exec4";
                } else {
                    assumption = "recommended4";
                }
            }
            fileExtension.Assumption = assumption;

            // get real destination
            var appPath = fileExtension.AppPath;
            if (appPath == "") {
                // Attempt to resolve from MS Store packages
                if (appPath == "") {
                    // Check if HKCR\<progId> exists
                    if (GatheredData.HKCR.HasDir(fileExtension.WinApiEntry.Progid)) {
                        // take HKCR\<progId>\shell\open\packageId
                        var id = GatheredData.HKCR.GetKey(fileExtension.WinApiEntry.Progid + "\\shell\\open\\PackageId");
                        if (id != "") {
                            // Use that to index into PackageRepository
                            if (GatheredData.HKCR_PackageRepository.HasDir(id)) {
                                //GatheredData.ShlwapiAssoc[progid];
                                appPath = GatheredData.HKCR_PackageRepository.GetKey(id + "\\PackageRootFolder");
                                //Console.WriteLine("YYYY: " + appPath);
                            }
                        }
                    }
                }

                // ???
                if (appPath == "") {
                    // Content-Type -> Media player related
                    if (Registry.getRootContentType(fileExtension.Extension) != "") {
                        var exec = Registry.ContentTypeExec(Registry.getRootContentType(fileExtension.Extension));
                        appPath = exec;
                    }
                }

                if (appPath == "") {
                    // Windows SystemApps related (Userchoice)
                    if (Registry.getUserChoice(fileExtension.Extension) != "") {
                        var exec = Registry.GetSystemApp(Registry.getUserChoice(fileExtension.Extension));
                        appPath += exec;
                    }

                }
                if (appPath == "") {
                    // Windows SystemApps related ()
                    if (Registry.countRootProgids(fileExtension.Extension) == 1) {
                        var exec = Registry.GetSystemApp(Registry.getRootProgid(fileExtension.Extension));
                        appPath += exec;
                    }
                }

                fileExtension.AppPath = appPath;
            }
        }
    }
}