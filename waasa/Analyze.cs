﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace waasa
{

    public class _FileExtensionDebug
    {
        public String Extension { get; set; }
        public String Result { get; set; }
        public String Assumption { get; set; }

        public bool hasXml { get; set; }
        public bool hasUserChoice { get; set; }
        public int cntUserProgids { get; set; }
        public int cntRootProdis { get; set; }
        public bool hasRootDefault { get; set; }
        public bool hasValidRootProgidsToasts { get; set; }
        public bool validTrippleNames { get; set; }
        public bool isValidRootDefault { get; set; }
    }


	public class _FileExtension
	{
		public string Extension { get; set; }
        public string Result { get; set; }
		public string Assumption { get; set; }
	}


	public class Analyze
	{
		public _GatheredData GatheredData { get; set; }
		public List<_FileExtension> FileExtensions { get; } = new List<_FileExtension>();


		public Analyze(_GatheredData gatheredData)
		{
			GatheredData = gatheredData;
		}


        public List<_FileExtensionDebug> GetDebug()
        {
            List<_FileExtensionDebug> result = new List<_FileExtensionDebug>();
            foreach (var extension in GatheredData.ListedExtensions) {
                string assumption = AnalyzeExtension(extension);

                _FileExtensionDebug entry = new _FileExtensionDebug();
                entry.Extension = extension;
                entry.Assumption = assumption;
                entry.hasXml = hasXml(extension);
                entry.hasUserChoice = hasUserChoice(extension);
                entry.cntUserProgids = countUserOpenWithProgids(extension);
                entry.cntRootProdis = countRootProgids(extension);
                entry.hasRootDefault = hasRootDefault(extension);
                entry.hasValidRootProgidsToasts = hasValidRootProgidsToasts(extension);
                entry.validTrippleNames = validTrippleNames(extension);
                entry.isValidRootDefault = isValidRootDefault(extension);

                result.Add(entry);
            }
            return result;
        }


		public void AnalyzeAll()
        {
			foreach(var extension in GatheredData.ListedExtensions) {
				string assumption = AnalyzeExtension(extension);
				_FileExtension fileExtension = new _FileExtension();
				fileExtension.Extension = extension;
                fileExtension.Assumption = assumption;
                FileExtensions.Add(fileExtension);
            }
        }


        public void AnalyzeSingle(string extension)
        {
            string assumption = AnalyzeExtension(extension);
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Extension = extension;
            fileExtension.Assumption = assumption;
            FileExtensions.Add(fileExtension);
        }


        private bool hasXml(string extension)
        {
            foreach(var xml in GatheredData.AppAssocXml) {
                if (xml.Extension == extension) {
                    return true;
                }
            }

            foreach (var xml in GatheredData.DefaultAssocXml) {
                if (xml.Extension == extension) {
                    return true;
                }
            }

            return false;
        }


        // HK_LocalUser
        private bool hasUserChoice(string extension)
        {
            // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                ///if (explorerExtensionDir.SubDirectories.ContainsKey("UserChoice")) {
                if (explorerExtensionDir.HasDir("UserChoice")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\UserChoice\
                    var userChoiceDir = explorerExtensionDir.GetDir("UserChoice");

                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\UserChoice\Progid
                    if (userChoiceDir.Keys.ContainsKey("ProgId")) {
                        return true;
                    }
                }
            }
            return false;
        }


        // HK_LocalUser
        private int countUserOpenWithProgids(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                if (explorerExtensionDir.HasDir("OpenWithProgids")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithProgids");
                    return openWithProgidsDir.Keys.Count;
                }
            }
            return 0;
        }


        // HK_CurrentRoot
        private int countRootProgids(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithProgids")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithProgids");
                    return openWithProgidsDir.Keys.Count;
                }
            }
            return 0;
        }


        // HK_CurrentRoot
        private bool hasRootDefault(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\(Default)
                if (rootExtensionDir.Keys.ContainsKey("")) {
                    var def = rootExtensionDir.Keys[""];
                    if (def != "") {
                        return true;
                    }
                }
            }
            return false;
        }


        // HK_CurrentRoot
        private bool isValidRootDefault(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\(Default)
                if (rootExtensionDir.Keys.ContainsKey("")) {
                    var def = rootExtensionDir.Keys[""];
                    if (def != "") {
                        // We have a objid. check if its valid
                        if (isExecutableObjid(def)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        private bool isExecutableObjid(string objid)
        {
            //Console.WriteLine("XXX: 1");

            if (GatheredData.HKCR.HasDir(objid)) {
                //Console.WriteLine("XXX: 2");
                var objdir = GatheredData.HKCR.GetDir(objid);
                if (objdir.HasDir("Shell")) {
                    //Console.WriteLine("XXX: 33");
                    var shell = objdir.GetDir("shell");

                    if (shell.HasDir("open")) {
                        //Console.WriteLine("XXX: 4");
                        var open = shell.GetDir("open");
                        if (open.HasDir("command")) {
                            //Console.WriteLine("XXX: 5");
                            var command = open.GetDir("command");
                            if (command.Keys.ContainsKey("") && command.Keys[""] != "") {
                                //Console.WriteLine("XXX 6: " + objid);

                                // check integrity of \Shell\(Default) so it points to a valid command
                                if (! shell.Keys.ContainsKey("")) {
                                    //Console.WriteLine(objid + ": No chosen");
                                }
                                if (shell.Keys.ContainsKey("") && shell.Keys[""].ToLower() != "open" ) {
                                    //Console.WriteLine(objid + ": No open: " + shell.Keys[""]);
                                }

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }


        // HK_CurrentRoot
        private bool hasValidRootProgidsToasts(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithProgids")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithProgids");

                    foreach (var entry in openWithProgidsDir.Keys) {
                        var objid = entry.Key;
                        //Console.WriteLine("A: " + entry.Key);
                        if (!hasToast(extension, objid)) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }


        public bool hasToast(string extension, string objid)
        {
            string keyToSearch = objid + "_" + extension;

            if (GatheredData.HKCU_ApplicationAssociationToasts.Keys.ContainsKey(keyToSearch)) {
                return true;
            } else {
                return false;
            }
        }


        public bool validTrippleNames(string extension)
        {
            var hkcuOpenWithObjid = "";
            var hkcrOpenWithObjid = "";
            var hkcrDefault = "";

            //if (AppLink.OpenWithProgids[0].Objid == AppData.OpenWithProgids[0].Objid && AppData.Default != null) {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                if (GatheredData.HKCU_ExplorerFileExts.GetDir(extension).HasDir("OpenWithProgids")) {
                    hkcuOpenWithObjid = GatheredData.HKCU_ExplorerFileExts.GetDir(extension)?.GetDir("OpenWithProgids")?.Keys.First().Key;
                }
            }

            if (GatheredData.HKCR.HasDir(extension)) {
                if (GatheredData.HKCR.GetDir(extension).HasDir("OpenWithProgids")) {
                    if (GatheredData.HKCR.GetDir(extension).GetDir("OpenWithProgids").Keys.Count == 1) {
                        hkcrOpenWithObjid = GatheredData.HKCR.GetDir(extension).GetDir("OpenWithProgids").Keys.First().Key;
                    }
                }
            }

            if (GatheredData.HKCR.HasDir(extension)) {
                if (GatheredData.HKCR.GetDir(extension).Keys.ContainsKey("")) {
                    hkcrDefault = GatheredData.HKCR.GetDir(extension).Keys[""];
                }
            }

            //var hkcuOpenWithObjid = GatheredData.HKCU_ExplorerFileExts.GetDir(extension)?.GetDir("OpenWithProgids")?.Keys.First().Key;
            //var hkcrOpenWithObjid = GatheredData.HKCR.GetDir(extension)?.GetDir("OpenWithProgids")?.Keys.First().Key;
            //var hkcrDefault = GatheredData.HKCR.GetDir(extension)?.Keys[""];

            if (hkcuOpenWithObjid == hkcrOpenWithObjid && hkcrOpenWithObjid == hkcrDefault
                && hkcrDefault != ""
            ) {
                return true;
            } else {
                return false;
            }
        }


        public bool isValidUserProgids(string extension)
        {
            string objid = "";
            _RegDirectory root = null;

            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                root = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);
            } else {
                return false;
            }

            if (root.HasDir("OpenWithProgids")) {
                var progids = root.GetDir("OpenWithProgids");
                if (progids.Keys.Count != 1) {
                    return false;
                } else {
                    objid = progids.Keys.First().Key;
                }
            }

            if (!hasToast(extension, objid)) {
                return false;
            }

            if (!isExecutableObjid(objid)) {
                return false;
            }

            return true;
        }
        

        public string AnalyzeExtension(string extension)
        {
            string ret = "";

            if (!hasValidRootProgidsToasts(extension)) {
                ret += "recommended0";
            }

            if (hasXml(extension)) {
                if (hasUserChoice(extension)) {
                    ret += "exec1.1";
                } else {
                    ret += "exec1.2"; // Unecessary?
                }
            } else {
                if (countUserOpenWithProgids(extension) == 1 && countRootProgids(extension) == 1) {
                    if (validTrippleNames(extension)) { 
                        ret += "exec2.2";
                    }
                }

                if (countRootProgids(extension) == 0) {
                    if (hasRootDefault(extension)) {
                        if (isValidRootDefault(extension)) {
                            ret += "exec4";
                        } else {
                            ret += "openwith2";
                        }
                    } else if (isValidUserProgids(extension)) { // only .jar
                        ret += "exec5";
                    } else {
                        ret += "openwith1";
                    }

                    return ret;
                } else {
                    ret += "exec3";
                }
            }

            return ret;
        }
    }
}