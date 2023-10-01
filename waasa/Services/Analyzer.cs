using System;
using System.Collections.Generic;
using waasa.Models;
using static System.Net.Mime.MediaTypeNames;


namespace waasa.Services {

    /// <summary>
    /// Merges GatheredData (and its simpleView) with Validator to produce FileExtensions 
    /// which contain all extensions and their associated path.
    /// </summary>
    public class Analyzer {
        private _GatheredData GatheredData { get; set; }
        private Validator Validator { get; set; }
        private GatheredDataSimpleView Registry { get; set; }
        private List<_FileExtension> FileExtensions { get; set; } = new List<_FileExtension>();


        public Analyzer() {
        }


        public void Load(_GatheredData gatheredData, Validator validator, GatheredDataSimpleView registry) {
            Console.WriteLine("Analyzer: Load");

            GatheredData = gatheredData;
            Validator = validator;
            Registry = registry;

            foreach (var extension in GatheredData.ListedExtensions) {
                var fileExtension = resolveExtension(extension);
                FileExtensions.Add(fileExtension);
            }
        }


        public List<_FileExtension> getResolvedFileExtensions() {
            return FileExtensions;
        }


        public _FileExtension resolveExtension(string extension) {
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
            analyzeExtension(fileExtension);

            return fileExtension;
        }


        // This implements the main algorithm to categorize file extension
        public void analyzeExtension(_FileExtension fileExtension) {
            // Logic to decide the assumption (how the file will be opened)
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
                    assumption = "exec1";
                } else {
                    assumption = "openwith2";
                }

            } else if (fileExtension.WinApiEntry.Command != "") {
                assumption = "exec2";
            } else {
                if (Registry.countUserOpenWithProgids(fileExtension.Extension) < 2) {
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
                        Console.WriteLine("A1: " + fileExtension.Extension + " " + exec);
                        appPath = exec;
                    }
                }

                if (appPath == "") {
                    // Windows SystemApps related (Userchoice)
                    if (Registry.getUserChoice(fileExtension.Extension) != "") {
                        var exec = Registry.GetSystemApp(Registry.getUserChoice(fileExtension.Extension));
                        Console.WriteLine("A2: " + fileExtension.Extension + " " + exec);
                        appPath += exec;
                    }

                }
                if (appPath == "") {
                    // Windows SystemApps related ()
                    if (Registry.countRootProgids(fileExtension.Extension) == 1) {
                        var exec = Registry.GetSystemApp(Registry.getRootProgid(fileExtension.Extension));
                        Console.WriteLine("A3: " + fileExtension.Extension + " " + exec);
                        appPath += exec;
                    }
                }

                fileExtension.AppPath = appPath;
            }
        }
    }
}