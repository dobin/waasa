using System.Collections.Generic;
using System.Linq;
using waasa.Services;


namespace waasa.Models {

    public class _FileExtensionDebug {
        public string Extension { get; set; } = "";
        public string Result { get; set; } = "";
        public string Assumption { get; set; } = "";
        public string AppName { get; set; } = "";
        public string AppPath { get; set; } = "";
        public string DdeExec { get; set; } = "";

        public string SysFileAssoc { get; set; } = "";
        public string WinFileAssoc { get; set; } = "";

        public string HKCU_SwCls { get; set; } = "";
        public bool HKLU_has { get; set; }

        public string User_Choice { get; set; } = "";
        public bool User_ChoiceExec { get; set; }
        public bool User_ChoiceToast { get; set; }

        public int User_CntProgids { get; set; }
        public string User_ProgidOne { get; set; } = "";
        public bool User_ProgidOneExec { get; set; }
        public bool User_ProgidOneToast { get; set; }
        public bool User_ProgIdsValid { get; set; }
        public bool User_ProgIdsValid2 { get; set; }
        public string User_CntList { get; set; } = "";
        public bool User_ValidList { get; set; }

        public string Root_Default { get; set; } = "";
        public bool Root_DefaultExec { get; set; }
        public bool Root_DefaultToast { get; set; }
        public int Root_CntProgids { get; set; }
        public bool Root_ProgidsValid { get; set; }
        public string Root_ProgidOne { get; set; } = "";
        public bool Root_ProgidOneExec { get; set; }
        public bool Root_ProgidOneToast { get; set; }
        public int Root_CountList { get; set; }
        public bool Root_ListIsValid { get; set; }
        public string Root_PersistentHandler { get; set; } = "";
        public string Root_ContentType { get; set; } = "";
        public string Root_PerceivedType { get; set; } = "";
    }


    /// <summary>
    /// An abstraction or interface to the data from the windows registry from GatheredData.
    /// </summary>
    public class GatheredDataSimpleView {
        private _GatheredData GatheredData;

        public GatheredDataSimpleView(_GatheredData gatheredData) {
            GatheredData = gatheredData;
        }


        public List<_FileExtensionDebug> GetFileExtensionDebug(List<_FileExtension> fileExtensions) {
            List<_FileExtensionDebug> result = new List<_FileExtensionDebug>();

            foreach (var fileExtension in fileExtensions) {
                _FileExtensionDebug entry = new _FileExtensionDebug();
                entry.Extension = fileExtension.Extension;
                entry.Result = fileExtension.Result;
                entry.Assumption = fileExtension.Assumption;
                entry.AppName = fileExtension.AppName;
                entry.AppPath = fileExtension.CmdLine;
                entry.DdeExec = GatheredData.WinapiData[fileExtension.Extension].DDECommand;

                var extension = fileExtension.Extension;

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
                entry.Root_ProgidOne = getRootProgid(extension);
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


        public string hasHKCUSwCls(string extension) {
            if (GatheredData.HKCU_SoftwareClasses.HasDir(extension)) {
                return "yes";
            }
            return "";
        }


        public bool hasHKLU(string extension) {
            if (GatheredData.HKCU_ExplorerFileExts.HasDir(extension)) {
                return true;
            } else {
                return false;
            }
        }


        public string getUserChoice(string extension) {
            string path = string.Format("{0}\\UserChoice\\Progid", extension);
            var res = GatheredData.HKCU_ExplorerFileExts.GetKey(path);
            return res;
        }

        public int countUserOpenWithProgids(string extension) {
            string path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (res == null) {
                return 0;
            }
            return res.Keys.Count;

        }

        public string getUserOpenWithProgids(string extension) {
            if (countUserOpenWithProgids(extension) == 1) {
                string path = string.Format("{0}\\OpenWithProgids", extension);
                var res = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
                if (res != null) {
                    return res.Keys.First().Key;
                }
            }
            return "";
        }

        public bool allUserProgidsValid(string extension) {
            string path = string.Format("{0}\\OpenWithProgids", extension);
            var openWithProgidsDir = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (openWithProgidsDir == null) {
                return true;
            }

            foreach (var openWithProgid in openWithProgidsDir.Keys) {
                var objid = openWithProgid.Key;

                if (!hasToast(extension, objid)) {
                    return false;
                }
            }

            return true;
        }


        // HK_LocalUser
        public string countUserOpenWithList(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var res = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (res == null) {
                return "";
            }
            return res.Keys.Count.ToString();
        }

        public string getUserOpenWithList(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var openWithListDir = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (openWithListDir == null || openWithListDir.Keys.Count != 1) {
                return "";
            }
            return openWithListDir.Keys.First().Key;
        }

        public bool isUserListValid(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var openWithProgidsDir = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (openWithProgidsDir == null) {
                return true;
            }
            foreach (var k in openWithProgidsDir.Keys) {
                if (k.Key.ToLower() == "MRUList") {
                    continue;
                }

                var app = k.Value;
                if (!appExists(app)) {
                    return false;
                }
            }

            return true;
        }

        // HK_CurrentRoot
        public int countRootProgids(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return 0;
            }
            return res.Keys.Count;
        }

        public string getRootProgid(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null || res.Keys.Count != 1) {
                return "";
            }
            return res.Keys.First().Key;
        }

        public bool allRootProgidsValid(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return true;
            }
            foreach (var k in res.Keys) {
                var objid = k.Key;

                if (!hasToast(extension, objid)) {
                    return false;
                }
            }
            return true;
        }

        // HK_CurrentRoot
        public int countRootOpenWithList(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return 0;
            }
            return res.Keys.Count;
        }

        public string getRootOpenWithList(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null || res.Keys.Count != 1) {
                return "";
            }
            return res.Keys.First().Key;
        }

        public bool isRootListValid(string extension) {
            var path = string.Format("{0}\\OpenWithList", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return true;
            }
            foreach (var k in res.Keys) {
                if (k.Key.ToLower() == "MRUList") {
                    continue;
                }

                var app = k.Value;
                if (!appExists(app)) {
                    return false;
                }
            }
            return true;
        }

        public string GetSystemApp(string objid) {
            // PackageID
            if (!GatheredData.HKCR.HasDir(objid)) {
                return "";
            }

            var shellopenPackageid = string.Format("{0}\\shell\\open\\PackageId", objid);
            var packageid = GatheredData.HKCR.GetKey(shellopenPackageid);
            if (packageid == null) {
                return "";
            }

            // Check if its a package
            // Computer\HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\
            //   Packages\Microsoft.Windows.SecHealthUI_10.0.19041.1865_neutral__cw5n1h2txyewy
            var path = string.Format("{0}", packageid);
            var res = GatheredData.HKCR_PackageRepository.GetDir(path);
            if (res == null) {
                return "";
            }
            if (res.Keys.ContainsKey("Path")) {
                return "? " + res.Keys["Path"];
            }

            return "";
        }

        public bool appExists(string objid) {
            var path = string.Format("Applications\\{0}", objid);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return false;
            }
            return true;
        }

        // HK_CurrentRoot
        public bool hasRootDefault(string extension) {
            var ret = GatheredData.HKCR.GetKey(extension + "\\(Default)");
            if (ret == null) {
                return false;
            }
            if (ret != "") {
                return true;
            }
            return false;
        }

        public string getRootDefault(string extension) {
            var ret = GatheredData.HKCR.GetKey(extension + "\\(Default)");
            return ret;
        }

        // HK_CurrentRoot
        public bool isValidRootDefault(string extension) {
            var def = GatheredData.HKCR.GetKey(extension + "\\(Default)");
            if (isExecutableObjid(def) && hasToast(extension, def)) {
                return true;
            }
            return false;
        }

        public string getRootPersistentHandler(string extension) {
            var path = string.Format("{0}\\PersistentHandler\\(Default)", extension);
            var res = GatheredData.HKCR.GetKey(path);
            return res;
        }

        public string ContentTypeExec(string mimetype) {
            var path = string.Format("MIME\\Database\\Content Type\\{0}\\CLSID", mimetype);
            var clsid = GatheredData.HKCR.GetKey(path);
            var ret = getClsidExe(clsid);
            return "? " + ret;
        }

        public string getClsidExe(string clsid) {
            var path = string.Format("CLSID\\{0}\\InprocServer32\\(Default)", clsid);
            var ret = GatheredData.HKCR.GetKey(path);
            return ret;
        }

        public string getRootContentType(string extension) {
            var path = string.Format("{0}\\Content Type", extension);
            var res = GatheredData.HKCR.GetKey(path);
            return res;
        }

        public string getRootPerceivedType(string extension) {
            var path = string.Format("{0}\\PerceivedType", extension);
            var res = GatheredData.HKCR.GetKey(path);
            return res;
        }

        public string GetExecutableForObjid(string objid) {
            var path = string.Format("{0}\\shell\\open\\command\\(Default)", objid);
            var res = GatheredData.HKCR.GetKey(path);
            return res;
        }


        public bool isExecutableObjid(string objid) {
            var path = string.Format("{0}\\shell\\open\\command\\(Default)", objid);
            var res = GatheredData.HKCR.GetKey(path);
            if (res == null) {
                return false;
            }
            return true;
        }

        public bool hasValidRootProgidsToasts(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return true;
            }
            foreach (var k in res.Keys) {
                var objid = k.Key;
                if (!hasToast(extension, objid)) {
                    return false;
                }
            }
            return true;
        }

        public bool hasToast(string extension, string objid) {
            string keyToSearch = objid + "_" + extension;
            if (GatheredData.HKCU_ApplicationAssociationToasts.Keys.ContainsKey(keyToSearch)) {
                return true;
            } else {
                return false;
            }
        }

        public bool isValidUserProgids(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCU_ExplorerFileExts.GetDir(path);
            if (res == null) {
                return false;
            }
            if (res.Keys.Count != 1) {
                return false;
            }
            var objid = res.Keys.First().Key;
            if (!hasToast(extension, objid)) {
                return false;
            }
            if (!isExecutableObjid(objid)) {
                return false;
            }
            return true;
        }

        public string getSysFileAssoc(string extension) {
            var path = string.Format("{0}\\shellex\\");
            var res = GatheredData.HKCR_SystemFileAssociations.GetDir(path);
            if (res == null) {
                return "";
            }
            return "shell";
        }

        public string getWinFileAssoc(string extension) {
            var res = GatheredData.HKCR_FileTypeAssociations.GetDir(extension);
            if (res == null) {
                return "";
            }
            var d = GatheredData.HKCR_FileTypeAssociations.GetDir(extension);
            if (d == null) {
                return "";
            } else if (d.SubDirectories.Count == 1) {
                return d.SubDirectories.First().Key;
            } else {
                return d.SubDirectories.Count.ToString();
            }
        }

        public bool isValidRootProgids(string extension) {
            var path = string.Format("{0}\\OpenWithProgids", extension);
            var res = GatheredData.HKCR.GetDir(path);
            if (res == null) {
                return false;
            }
            if (res.Keys.Count != 1) {
                return false;
            }
            var objid = res.Keys.First().Key;

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
