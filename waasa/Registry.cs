using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waasa
{

    public class _FileExtensionDebug
    {
        // From _FileExtension
        public String Extension { get; set; }
        public String Result { get; set; }
        public String Assumption { get; set; }
        public String AppName { get; set; }
        public String AppPath { get; set; }
        public String DdeExec { get; set; }

        public string SysFileAssoc { get; set; }
        public string WinFileAssoc { get; set; }

        public string HKCU_SwCls { get; set; }
        public bool HKLU_has { get; set; }

        public string User_Choice { get; set; }
        public bool User_ChoiceExec { get; set; }
        public bool User_ChoiceToast { get; set; }

        public int User_CntProgids { get; set; }
        public string User_ProgidOne { get; set; }
        public bool User_ProgidOneExec { get; set; }
        public bool User_ProgidOneToast { get; set; }
        public bool User_ProgIdsValid { get; set; }
        public bool User_ProgIdsValid2 { get; set; }
        public string User_CntList { get; set; }
        public bool User_ValidList { get; set; }

        public string Root_Default { get; set; }
        public bool Root_DefaultExec { get; set; }
        public bool Root_DefaultToast { get; set; }
        public int Root_CntProgids { get; set; }
        public bool Root_ProgidsValid { get; set; }
        public string Root_ProgidOne { get; set; }
        public bool Root_ProgidOneExec { get; set; }
        public bool Root_ProgidOneToast { get; set; }
        public int Root_CountList { get; set; }
        public bool Root_ListIsValid { get; set; }
        public string Root_PersistentHandler { get; set; }
        public string Root_ContentType { get; set; }
        public string Root_PerceivedType { get; set; }
    }


    public class Registry
    {
        private _GatheredData GatheredData;


        public Registry(_GatheredData gatheredData)
        {
            GatheredData = gatheredData;
        }


        public List<_FileExtensionDebug> GetFileExtensionDebug(List<_FileExtension> fileExtensions)
        {
            List<_FileExtensionDebug> result = new List<_FileExtensionDebug>();

            foreach(var fileExtension in fileExtensions) {
                _FileExtensionDebug entry = new _FileExtensionDebug();
                entry.Extension = fileExtension.Extension;
                entry.Result = fileExtension.Result;
                entry.Assumption = fileExtension.Assumption;
                entry.AppName = fileExtension.AppName;
                entry.AppPath = fileExtension.AppPath;
                entry.DdeExec = GatheredData.ShlwapiAssoc[fileExtension.Extension].DDECommand;

                var extension = fileExtension.Extension;

                /*
                entry.Xml1Choice = getXml1(extension);
                entry.Xml1Exec = isExecutableObjid(entry.Xml1Choice);
                entry.Xml1Toast = hasToast(extension, entry.Xml1Choice);

                entry.Xml2Choice = getXml2(extension);
                entry.Xml2Exec = isExecutableObjid(entry.Xml2Choice);
                entry.Xml2Toast = hasToast(extension, entry.Xml2Choice);
                */

                entry.SysFileAssoc = getSysFileAssoc(extension);
                entry.WinFileAssoc = getWinFileAssoc(extension);

                entry.User_Choice = getUserChoice(extension);
                entry.User_ChoiceExec = isExecutableObjid(entry.User_Choice);
                entry.User_ChoiceToast = hasToast(extension, entry.User_Choice);

                entry.User_CntProgids = countUserOpenWithProgids(extension);
                entry.User_ProgidOne = getUserOpenWithProgids(extension);
                entry.User_ProgidOneExec = isExecutableObjid(entry.User_ProgidOne);
                entry.User_ProgidOneToast = hasToast(extension, entry.User_ProgidOne);

                entry.HKCU_SwCls = hasHKCUSwCls(extension);

                entry.User_CntList = countUserOpenWithList(extension);
                entry.User_ProgIdsValid = allUserProgidsValid(extension);
                entry.Root_ProgidsValid = allRootProgidsValid(extension);
                entry.User_ValidList = isUserListValid(extension);
                entry.Root_ListIsValid = isRootListValid(extension);
                entry.HKLU_has = hasHKLU(extension);

                entry.Root_CntProgids = countRootProgids(extension);
                entry.Root_ProgidOne = getRootProgids(extension);
                entry.Root_ProgidOneExec = isExecutableObjid(entry.Root_ProgidOne);
                entry.Root_ProgidOneToast = hasToast(extension, entry.Root_ProgidOne);

                entry.Root_CountList = countRootOpenWithList(extension);
                entry.Root_PersistentHandler = getRootPersistentHandler(extension);
                entry.Root_ContentType = getRootContentType(extension);
                entry.Root_PerceivedType = getRootPerceivedType(extension);

                entry.Root_Default = getRootDefault(extension);
                entry.Root_DefaultExec = isExecutableObjid(entry.Root_Default);
                entry.Root_DefaultToast = hasToast(extension, entry.Root_Default);

                result.Add(entry);
            }

            return result;
        }


        public string hasHKCUSwCls(string extension)
        {
            if (GatheredData.HKCU_SoftwareClasses.HasDir(extension)) {
                return "yes";
            }
            return "";
        }

        public string getXml1(string extension)
        {
            foreach (var xml in GatheredData.AppAssocXml) {
                if (xml.Extension == extension) {
                    return xml.Progid;
                }
            }
            return "";
        }

        public string getXml2(string extension)
        {
            foreach (var xml in GatheredData.DefaultAssocXml) {
                if (xml.Extension == extension) {
                    return xml.Progid;
                }
            }
            return "";
        }

        public bool hasXml(string extension)
        {
            foreach (var xml in GatheredData.AppAssocXml) {
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

        public bool hasHKLU(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                return true;
            } else {
                return false;
            }
        }


        // HK_LocalUser
        public string getUserChoice(string extension)
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

                        var progid = userChoiceDir.Keys["ProgId"];
                        return progid;
                        //return true;
                    }
                }
            }
            return "";
        }


        // HK_LocalUser
        public int countUserOpenWithProgids(string extension)
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
        public string getUserOpenWithProgids(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                if (explorerExtensionDir.HasDir("OpenWithProgids")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithProgids");
                    if (openWithProgidsDir.Keys.Count == 1) {
                        return openWithProgidsDir.Keys.First().Key;
                    }
                }
            }
            return "";
        }
        public bool allUserProgidsValid(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                if (explorerExtensionDir.HasDir("OpenWithProgids")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithProgids");

                    foreach (var openWithProgid in openWithProgidsDir.Keys) {
                        var objid = openWithProgid.Key;

                        if (!hasToast(extension, objid)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        // HK_LocalUser
        public string countUserOpenWithList(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);
                if (explorerExtensionDir.HasDir("OpenWithList")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithList");

                    if (openWithProgidsDir.Keys.Count == 2) {
                        foreach (var l in openWithProgidsDir.Keys) {
                            if (l.Key.ToLower() != "MRUList") {
                                return l.Value;
                            }
                        }
                    }

                    return openWithProgidsDir.Keys.Count.ToString();
                }
            }
            return "";
        }
        public string getUserOpenWithList(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                if (explorerExtensionDir.HasDir("OpenWithList")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithList");
                    if (openWithProgidsDir.Keys.Count == 1) {
                        return openWithProgidsDir.Keys.First().Key;
                    }
                }
            }
            return "";
        }
        public bool isUserListValid(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithList")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithList");

                    foreach (var k in openWithProgidsDir.Keys) {
                        if (k.Key.ToLower() == "MRUList") {
                            continue;
                        }

                        var app = k.Value;
                        if (!appExists(app)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // HK_CurrentRoot
        public int countRootProgids(string extension)
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
        public string getRootProgids(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithProgids")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithProgids");

                    if (openWithProgidsDir.Keys.Count == 1) {
                        return openWithProgidsDir.Keys.First().Key;
                    }
                }
            }
            return "";
        }
        public bool allRootProgidsValid(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithProgids")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithProgids");

                    foreach (var openWithProgid in openWithProgidsDir.Keys) {
                        var objid = openWithProgid.Key;

                        if (!hasToast(extension, objid)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        // HK_CurrentRoot
        public int countRootOpenWithList(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithList")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithList");
                    return openWithProgidsDir.Keys.Count;
                }
            }
            return 0;
        }
        public string getRootOpenWithList(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithList")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithList");

                    if (openWithProgidsDir.Keys.Count == 1) {
                        return openWithProgidsDir.Keys.First().Key;
                    }
                }
            }
            return "";
        }
        public bool isRootListValid(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithList")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithList");

                    foreach (var k in openWithProgidsDir.Keys) {
                        if (k.Key.ToLower() == "MRUList") {
                            continue;
                        }

                        var app = k.Value;
                        if (!appExists(app)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public bool appExists(string objid)
        {
            var root = GatheredData.HKCR.GetDir("Applications");
            if (root.HasDir(objid)) {
                var app = root.GetDir(objid);
                if (app.HasDir("shell")) {
                    return true;
                }
            }
            return false;
        }

        // HK_CurrentRoot
        public bool hasRootDefault(string extension)
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
        public string getRootDefault(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\(Default)
                if (rootExtensionDir.Keys.ContainsKey("")) {
                    var def = rootExtensionDir.Keys[""];
                    if (def != "") {
                        return def;
                    }
                }
            }
            return "";
        }

        // HK_CurrentRoot
        public bool isValidRootDefault(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\(Default)
                if (rootExtensionDir.Keys.ContainsKey("")) {
                    var def = rootExtensionDir.Keys[""];
                    if (def != "") {
                        // We have a objid. check if its valid
                        if (isExecutableObjid(def) && hasToast(extension, def)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public string getRootPersistentHandler(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\PersistentHandler\
                if (rootExtensionDir.HasDir("PersistentHandler")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("PersistentHandler");

                    if (openWithProgidsDir.Keys.ContainsKey("")) {
                        var persistentHandler = openWithProgidsDir.Keys[""];
                        return persistentHandler;
                    }
                }
            }
            return "";
        }

        public string getRootContentType(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                if (rootExtensionDir.Keys.ContainsKey("Content Type")) {
                    var contenttype = rootExtensionDir.Keys["Content Type"];
                    return contenttype;
                }
            }
            return "";
        }
        public string getRootPerceivedType(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                if (rootExtensionDir.Keys.ContainsKey("PerceivedType")) {
                    var contenttype = rootExtensionDir.Keys["PerceivedType"];
                    return contenttype;
                }
            }
            return "";
        }


        public bool isExecutableObjid(string objid)
        {
            //Console.WriteLine("XXX: 1");

            if (GatheredData.HKCR.HasDir(objid)) {
                //Console.WriteLine("XXX: 2");
                var objdir = GatheredData.HKCR.GetDir(objid);
                if (objdir.HasDir("Shell")) {
                    //Console.WriteLine("XXX: 33");
                    var shell = objdir.GetDir("shell");

                    if (shell.Keys.ContainsKey("") && shell.Keys[""].ToLower() == "open") {
                    } else {
                        //Console.WriteLine("objid bad: " + objid);
                        //return false;
                    }

                    if (shell.HasDir("open")) {
                        //Console.WriteLine("XXX: 4");
                        var open = shell.GetDir("open");
                        if (open.HasDir("command")) {
                            //Console.WriteLine("XXX: 5");
                            var command = open.GetDir("command");
                            if (command.Keys.ContainsKey("") && command.Keys[""] != "") {
                                //Console.WriteLine("XXX 6: " + objid);

                                // check integrity of \Shell\(Default) so it points to a valid command
                                if (!shell.Keys.ContainsKey("")) {
                                    //Console.WriteLine(objid + ": No chosen");
                                }
                                if (shell.Keys.ContainsKey("") && shell.Keys[""].ToLower() != "open") {
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
        public bool hasValidRootProgidsToasts(string extension)
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

        public string getSysFileAssoc(string extension)
        {
            string ret = "";
            if (GatheredData.HKCR_SystemFileAssociations.HasDir(extension)) {
                ret += "yes:";

                var d = GatheredData.HKCR_SystemFileAssociations.GetDir(extension);
                if (d.HasDir("shellex")) {
                    ret += "shell";
                } else {
                    ret += "noshell";
                }
            }
            return ret;
        }

        public string getWinFileAssoc(string extension)
        {
            string ret = "";
            if (GatheredData.HKCR_FileTypeAssociations.HasDir(extension)) {
                ret += "yes:";

                var d = GatheredData.HKCR_FileTypeAssociations.GetDir(extension);
                if (d.SubDirectories.Count == 1) {
                    ret += d.SubDirectories.First().Key;
                } else {
                    ret += d.SubDirectories.Count;
                }
            }
            return ret;
        }


        public bool isValidRootProgids(string extension)
        {
            string objid = "";
            _RegDirectory root = null;

            if (GatheredData.HKCR.HasDir(extension)) {
                root = GatheredData.HKCR.GetDir(extension);
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

    }
}
