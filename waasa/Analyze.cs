using System;
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

        public string Xml1Choice { get; set; }
        public bool Xml1Exec { get; set; }
        public bool Xml1Toast { get; set; }

        public string Xml2Choice { get; set; }
        public bool Xml2Exec { get; set; }
        public bool Xml2Toast { get; set; }

        public string SysFileAssoc { get; set; }
        public string WinFileAssoc { get; set; }

        public string UserChoice { get; set; }
        public bool UserChoiceExec { get; set; }
        public bool UserChoiceToast { get; set; }

        public int cntUserProgids { get; set; }
        public string UserProgidOne { get; set; }
        public bool UserProgidOneExec { get; set; }
        public bool UserProgidOneToast { get; set; }

        public string HasHKCUSwCls { get; set; }

        public string cntUserList { get; set; }
        public bool UserProgIdsValid { get; set; }
        public bool RootProgidsValid { get; set; }
        public bool UserListIsValid { get; set; }
        public bool RootListIsValid { get; set; }
        public bool AAhasHKLU { get; set; }

        public int cntRootProgids { get; set; }
        public string RootProgidOne { get; set; }
        public bool RootProgidOneExec { get; set; }
        public bool RootProgidOneToast { get; set; }

        public int cntRootList { get; set; }
        public string RootPersistentHandler { get; set; }
        public string RootContentType { get; set; }
        public string RootPerceivedType { get; set; }

        public string RootDefault { get; set; }
        public bool RootDefaultExec { get; set; }
        public bool RootDefaultToast { get; set; }
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

                entry.Xml1Choice = getXml1(extension);
                entry.Xml1Exec = isExecutableObjid(entry.Xml1Choice);
                entry.Xml1Toast = hasToast(extension, entry.Xml1Choice);

                entry.Xml2Choice = getXml2(extension);
                entry.Xml2Exec = isExecutableObjid(entry.Xml2Choice);
                entry.Xml2Toast = hasToast(extension, entry.Xml2Choice);

                entry.SysFileAssoc = getSysFileAssoc(extension);
                entry.WinFileAssoc = getWinFileAssoc(extension);

                entry.UserChoice = getUserChoice(extension);
                entry.UserChoiceExec = isExecutableObjid(entry.UserChoice);
                entry.UserChoiceToast = hasToast(extension, entry.UserChoice);

                entry.cntUserProgids = countUserOpenWithProgids(extension);
                entry.UserProgidOne = getUserOpenWithProgids(extension);
                entry.UserProgidOneExec = isExecutableObjid(entry.UserProgidOne);
                entry.UserProgidOneToast = hasToast(extension, entry.UserProgidOne);

                entry.HasHKCUSwCls = hasHKCUSwCls(extension);

                entry.cntUserList = countUserOpenWithList(extension);
                entry.UserProgIdsValid = allUserProgidsValid(extension);
                entry.RootProgidsValid = allRootProgidsValid(extension);
                entry.UserListIsValid = isUserListValid(extension);
                entry.RootListIsValid = isRootListValid(extension);
                entry.AAhasHKLU = hasHKLU(extension);

                entry.cntRootProgids = countRootProgids(extension);
                entry.RootProgidOne = getRootProgids(extension);
                entry.RootProgidOneExec = isExecutableObjid(entry.RootProgidOne);
                entry.RootProgidOneToast = hasToast(extension, entry.RootProgidOne);

                entry.cntRootList = countRootOpenWithList(extension);
                entry.RootPersistentHandler = getRootPersistentHandler(extension);
                entry.RootContentType = getRootContentType(extension);
                entry.RootPerceivedType = getRootPerceivedType(extension);

                entry.RootDefault = getRootDefault(extension);
                entry.RootDefaultExec = isExecutableObjid(entry.RootDefault);
                entry.RootDefaultToast = hasToast(extension, entry.RootDefault);

                result.Add(entry);
            }
            return result;
        }


        public void AnalyzeAll()
        {
            foreach (var extension in GatheredData.ListedExtensions) {
                string assumption = AnalyzeExtension(extension);
                _FileExtension fileExtension = new _FileExtension();
                fileExtension.Extension = extension;
                fileExtension.Assumption = assumption;
                FileExtensions.Add(fileExtension);
            }
        }

        public string hasHKCUSwCls(string extension)
        {
            if (GatheredData.HKCU_SoftwareClasses.HasDir(extension)) {
                return "yes";
            }

            return "";
        }

            public void AnalyzeSingle(string extension)
        {
            string assumption = AnalyzeExtension(extension);
            _FileExtension fileExtension = new _FileExtension();
            fileExtension.Extension = extension;
            fileExtension.Assumption = assumption;
            FileExtensions.Add(fileExtension);
        }


        private string getXml1(string extension)
        {
            foreach (var xml in GatheredData.AppAssocXml) {
                if (xml.Extension == extension) {
                    return xml.Progid;
                }
            }
            return "";
        }

        private string getXml2(string extension)
        {
            foreach (var xml in GatheredData.DefaultAssocXml) {
                if (xml.Extension == extension) {
                    return xml.Progid;
                }
            }
            return "";
        }

        private bool hasXml(string extension)
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

        private bool hasHKLU(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                return true;
            } else {
                return false;
            }
        }


        // HK_LocalUser
        private string getUserChoice(string extension)
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
        private string getUserOpenWithProgids(string extension)
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
        private bool allUserProgidsValid(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);

                if (explorerExtensionDir.HasDir("OpenWithProgids")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithProgids");

                    foreach (var openWithProgid in openWithProgidsDir.Keys) {
                        var objid = openWithProgid.Key;

                        if (! hasToast(extension, objid)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }


        // HK_LocalUser
        private string countUserOpenWithList(string extension)
        {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\
                var explorerExtensionDir = GatheredData.HKCU_ExplorerFileExts.GetDir(extension);
                if (explorerExtensionDir.HasDir("OpenWithList")) {
                    // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\<extension>\OpenWithProgids\
                    var openWithProgidsDir = explorerExtensionDir.GetDir("OpenWithList");

                    if (openWithProgidsDir.Keys.Count == 2) {
                        foreach(var l in openWithProgidsDir.Keys) {
                            if (l.Key.ToLower() != "MRUList") {
                                return "A: " + l.Value;
                            }
                        }
                    }

                    return "B: " + openWithProgidsDir.Keys.Count.ToString();
                }
            }
            return "C";
        }
        private string getUserOpenWithList(string extension)
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
        private bool isUserListValid(string extension)
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
        private string getRootProgids(string extension)
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
        private bool allRootProgidsValid(string extension)
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
        private int countRootOpenWithList(string extension)
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
        private string getRootOpenWithList(string extension)
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
        private bool isRootListValid(string extension)
        {
            if (GatheredData.HKCR.HasDir(extension)) {
                // HKCR\<extension>\
                var rootExtensionDir = GatheredData.HKCR.GetDir(extension);

                // HKCR\<extension>\OpenWithProgids\
                if (rootExtensionDir.HasDir("OpenWithList")) {
                    var openWithProgidsDir = rootExtensionDir.GetDir("OpenWithList");

                    foreach(var k in openWithProgidsDir.Keys) {
                        if (k.Key.ToLower() == "MRUList") {
                            continue;
                        }

                        var app = k.Value;
                        if (! appExists(app)) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool appExists(string objid)
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
        private string getRootDefault(string extension)
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
                        if (isExecutableObjid(def) && hasToast(extension, def)) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private string getRootPersistentHandler(string extension)
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

        private string getRootContentType(string extension)
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
        private string getRootPerceivedType(string extension)
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


        private bool isExecutableObjid(string objid)
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

        public string AnalyzeExtension00(string extension)
        {
            string x;

            // One
            if (getUserChoice(extension) == ""
                && countUserOpenWithProgids(extension) == 0
                && countRootProgids(extension) == 1) 
            {
                if (getRootPerceivedType(extension) != "") {
                    return "recommended1";
                } else {
                    return "exec1";
                }
            }

            // Two
            if (getUserChoice(extension) == ""
                && countUserOpenWithProgids(extension) == 0
                && countRootProgids(extension) == 0
                && hasRootDefault(extension)) {
                if (getRootPerceivedType(extension) != "") {
                    return "recommended2";
                } else {
                    return "exec2";
                }
            }


            return "recommended_end";
        }

        public string AnalyzeExtension(string extension) {
            string x;
            string ret = "";

            if (getXml1(extension) != "") {
                x = getXml1(extension);
                //if (isExecutableObjid(x) && hasToast(extension, x)) {
                if (hasToast(extension, x)) {

                    // may also be: RootDefaultToast == False
                    if (allUserProgidsValid(extension)) {
                        //return "exec1";

                        if (allRootProgidsValid(extension)) {
                            return "exec1";
                        } else {
                            return "recommended1.1";
                        }
                    } else {

                        return "recommended1.2";
                    }

                }
            }

            if (getXml2(extension) != "") {
                x = getXml2(extension);
                if (isExecutableObjid(x) && hasToast(extension, x)) {
                    return "exec2"; // none
                }
            }

            /*** XML finished ***/

            if (getUserChoice(extension) != "") {
                x = getUserChoice(extension);
                if (isExecutableObjid(x) && hasToast(extension, x)) {
                    return "exec3"; // 1 wrong
                }
            }

            // text exception
            if (getRootPerceivedType(extension) == "text" && getRootContentType(extension) == "text/plain") {
                return "recommended3";
            }

            if (getUserOpenWithProgids(extension) != "") {
                x = getUserOpenWithProgids(extension);
                if (isExecutableObjid(x) && hasToast(extension, x)) {

                    // Weird special case
                    //if (getRootProgids(extension) != "" && ! hasToast(extension, getRootProgids(extension))) {
                    //    return "recommended0";
                    //}
                    if (allRootProgidsValid(extension)) {
                        return "exec4";
                    } else {
                        return "recommended4";
                    }
                }
            }

            if (getRootProgids(extension) != "") {
                x = getRootProgids(extension);
                if (isExecutableObjid(x) && hasToast(extension, x)) {

                    //if (getRootPerceivedType(extension) == "text" && getRootContentType(extension) == "text/plain") {
                    if (getRootPerceivedType(extension) == "text") {
                        return "recommended5";
                    } else {
                        return "exec5";
                    }
                    
                }
            }

            if (getRootProgids(extension) != "" && hasToast(extension, getRootProgids(extension))) {
                /*
                if (countUserOpenWithList(extension) > 1) {
                    return "recommended6";
                } else {
                    return "exec6";
                }*/
                return "exec6";
            }

            
            if (countUserOpenWithProgids(extension) == 1 && countRootProgids(extension) == 1) {
                if (getUserOpenWithProgids(extension) == getRootProgids(extension) 
                    && getRootProgids(extension) == getRootDefault(extension)) 
                {
                    return "exec10";
                }
            }

            if (getRootDefault(extension) != "" && isExecutableObjid(getRootDefault(extension))) {

                if (countRootProgids(extension) == 0) {
                    return "exec7";
                } else {
                    return "recommended7";
                }
                
            }

            if (allRootProgidsValid(extension) && hasToast(extension, getRootDefault(extension))) {
                return "exec8";
            }

            //if (hasToast(extension, getRootDefault(extension))) {
            //    return "";
            //}

            if (! hasHKLU(extension)) {
                // Only one RootProgids
                if (countRootProgids(extension) == 1) {
                    if (! isExecutableObjid(getRootProgids(extension))) {
                        // weirdly, it is executen if there is NO toat...
                        return "exec8";
                    }

                // NO root progids
                } else if (countRootProgids(extension) == 0) {
                    if (hasRootDefault(extension)) {
                        return "exec9";
                    }
                }
            }


            return "openwithEnd";
            //return ret;
        }

        public string AnalyzeExtension2(string extension)
        {
            string ret = "";
            string objid = null;

            /*if (!hasValidRootProgidsToasts(extension)) {
                ret += "recommended0"; // errors: .png, .wm (they are: recommended0 + exec 1.1)
            }*/

            if (hasXml(extension)) {
                if (getUserChoice(extension) != "") {
                    ret += "1.1";
                    objid = getUserChoice(extension);
                    if (isExecutableObjid(objid) && hasToast(extension, objid)) {
                        ret += "exec1.1";
                    } else {
                        ret += "exec1.1b";
                    }
                    
                } else {
                    ret += "exec1.2"; // Unecessary?
                }
            } else {
                // New: even without XML, we should check for userchoice
                if (getUserChoice(extension) != "") {

                    objid = getUserChoice(extension);
                    if (isExecutableObjid(objid) && hasToast(extension, objid)) {
                        ret += "exec2.1";
                    } else {
                        ret += "exec2.1b";
                    }

                }
                if (isValidUserProgids(extension)) {
                    ret += "exec2"; // errors: .py, .cab
                } else {
                    if (isValidRootProgids(extension)) { // also checks Toast, Executable
                        ret += "exec3"; // errors: .aspx
                    } else {
                        if (isValidRootDefault(extension)) { // also checks Toast, Executable
                            ret += "exec4"; // no errors
                        } else {
                            if (countUserOpenWithProgids(extension) == 0 && countRootProgids(extension) == 1) {
                                ret += "exec5"; // ok
                            } else {
                                ret += "openwith5"; // errors: .odc, .ttf
                            }
                        }
                    }
                }

                //ret += "recommended";

                /*
                // OLD 2
                if (isValidUserProgids(extension)) {
                    ret += "exec2"; // errors: .py, .cab
                } else {
                    if (isValidRootProgids(extension)) {
                        ret += "exec3"; // errors: .aspx
                    } else {
                        if (isValidRootDefault(extension)) {
                            ret += "exec4"; // no errors
                        } else {
                            if (countUserOpenWithProgids(extension) == 0 && countRootProgids(extension) == 1) {
                                ret += "exec5"; // ok
                            } else {
                                ret += "openwith5"; // errors: .odc, .ttf
                            }
                        }
                    }
                }
                */

                /*
                // OLD 1
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
                }*/
            }

            return ret;
        }
    }
}