using System;
using System.Collections.Generic;
using waasa.Models;


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
                var fileExtension = AnalyzeSingle(extension);
                fileExtensions.Add(fileExtension);
            }
            return fileExtensions;
        }


        public _FileExtension AnalyzeSingle(string extension) {
            var data = AnalyzeExtension(extension);
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Result = Validator.GetEffectiveResultFor(extension);
            fileExtension.Extension = extension;
            fileExtension.Assumption = data.Item1;
            fileExtension.AppName = data.Item2;
            fileExtension.AppPath = data.Item3;

            return fileExtension;
        }


        public Winapi.WinapiEntry GetShlwapiBy(string ext) {
            if (GatheredData.WinapiData.ContainsKey(ext)) {
                return GatheredData.WinapiData[ext];
            } else {
                return null;
            }
        }


        // This implements the main algorithm to categorize file extension
        public Tuple<string, string, string> AnalyzeExtension(string extension) {
            var assoc = GetShlwapiBy(extension);

            string assumption = "";
            string appName = assoc.FriendlyAppName;
            string appPath = assoc.Command;
            string progId = assoc.Progid;
            string appId = assoc.AppId;

            if (assoc.FriendlyAppName.StartsWith("Pick an app")) {
                assumption = "openwith1";

            } else if (assoc.FriendlyAppName == "") {
                if (assoc.Command != "" && !Registry.isValidRootProgids(extension) && Registry.hasRootDefault(extension)) {
                    // May also use: Root_DefaultExec
                    // Basically just .cmd, .com
                    assumption = "exec2";
                } else {
                    assumption = "openwith2";
                }

            } else if (assoc.Command != "") {
                assumption = "exec3";
            } else {
                if (Registry.countUserOpenWithProgids(extension) < 2) {
                    assumption = "exec4";
                } else {
                    assumption = "recommended4";
                }
            }

            // get real destination
            if (appPath == "") {
                // Attempt to resolve from MS Store packages
                if (appPath == "") {
                    // Check if HKCR\<progId> exists
                    if (GatheredData.HKCR.HasDir(progId)) {
                        // take HKCR\<progId>\shell\open\packageId
                        var id = GatheredData.HKCR.GetKey(progId + "\\shell\\open\\PackageId");
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

                if (appPath == "") {
                    // Content-Type -> Media player related
                    if (Registry.getRootContentType(extension) != "") {
                        var exec = Registry.ContentTypeExec(Registry.getRootContentType(extension));
                        appPath = exec;
                    }
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
                        var exec = Registry.GetSystemApp(Registry.getRootProgid(extension));
                        appPath += exec;
                    }
                }

            }

            return new Tuple<string, string, string>(assumption, appName, appPath);
        }
    }
}