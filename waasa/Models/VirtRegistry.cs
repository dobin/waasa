using System;
using System.Collections.Generic;


namespace waasa.Models {

    /// <summary>
    /// Represents part of Windows Registry as data structure
    /// </summary>
    [Serializable]
    public class _RegDirectory {
        // The name of this directory (path)
        public string Name { get; set; }
        public Dictionary<string, _RegDirectory> SubDirectories { get; set; } = new Dictionary<string, _RegDirectory>();
        public Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();


        public _RegDirectory(string name) {
            Name = name;
        }


        // aaa\bbb\ccc
        public bool HasDir(string path) {
            if (path.Contains("\\")) {
                int i = path.IndexOf("\\");
                string currentDir = path.Substring(0, i);
                string restPath = path.Substring(i + 1);

                if (SubDirectories.ContainsKey(currentDir.ToLower())) {
                    var dir = SubDirectories[currentDir.ToLower()];
                    return dir.HasDir(restPath);
                } else {
                    return false;
                }
            } else {
                if (SubDirectories.ContainsKey(path.ToLower())) {
                    return true;
                } else {
                    return false;
                }
            }
        }


        public _RegDirectory GetDir(string path) {
            if (path.Contains("\\")) {
                int i = path.IndexOf("\\");
                string currentDir = path.Substring(0, i);
                string restPath = path.Substring(i + 1);

                if (SubDirectories.ContainsKey(currentDir.ToLower())) {
                    var dir = SubDirectories[currentDir.ToLower()];
                    return dir.GetDir(restPath);
                } else {
                    return null;
                }
            } else {
                if (SubDirectories.ContainsKey(path.ToLower())) {
                    return SubDirectories[path.ToLower()];
                } else {
                    return null;
                }
            }
        }


        public void AddDir(string name, _RegDirectory dir) {
            SubDirectories.Add(name.ToLower(), dir);
        }


        public void AddKey(string key, string value) {
            Keys.Add(key.ToLower(), value);
        }


        public string GetKey(string path) {
            path = path.ToLower();
            if (path.Contains("\\") && !path.StartsWith("\\")) {
                // Directories
                int i = path.IndexOf("\\");
                string currentDir = path.Substring(0, i);
                string restPath = path.Substring(i + 1);
                if (SubDirectories.ContainsKey(currentDir)) {
                    var dir = SubDirectories[currentDir];
                    return dir.GetKey(restPath);
                } else {
                    return "";
                }
            } else {
                // Key
                string keyname = path;
                // Clean
                if (keyname.StartsWith("\\")) {
                    keyname = keyname.Substring(1);
                }

                if (keyname == "(default)") {
                    keyname = "";
                }

                if (Keys.ContainsKey(keyname)) {
                    return Keys[keyname];
                } else {
                    return "";
                }
            }
        }


        public string toStr(int n) {
            string ret = "";

            string indent = new string(' ', n * 2);
            foreach (var key in Keys) {
                string realKey = "(Default)";
                if (key.Key != "") {
                    realKey = key.Key;
                }
                ret += string.Format("{0}{1}: {2}\n", indent, realKey, key.Value);
            }

            foreach (var dirEl in SubDirectories) {
                ret += string.Format("{0}{1}\\ \n", indent, dirEl.Key);
                var dir = dirEl.Value;
                ret += dir.toStr(n + 1);
            }

            return ret;
        }
    }

}
