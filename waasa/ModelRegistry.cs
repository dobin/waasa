using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace waasa
{
    [Serializable]
    public class _RegDirectory
    {
        // The name of this directory (path)
        public string Name { get; set; }

        public Dictionary<string, _RegDirectory> SubDirectories { get; set; } = new Dictionary<string, _RegDirectory>();
        public Dictionary<string, string> Keys { get; set;  } = new Dictionary<string, string>();

        public _RegDirectory(string name)
        {
            Name = name;
        }

        public bool HasDir(string name)
        {
            if (SubDirectories.ContainsKey(name.ToLower())) {
                return true;
            } else {
                return false;
            }
        }
        public _RegDirectory GetDir(string name)
        {
            if (SubDirectories.ContainsKey(name.ToLower())) {
                return SubDirectories[name.ToLower()];
            }

            return null;
        }
        public void AddDir(string name, _RegDirectory dir)
        {
            SubDirectories.Add(name.ToLower(), dir);
        }

        public string toStr(int n)
        {
            string ret = "";

            string indent = new String(' ', n*2);
            foreach (var key in Keys) {
                string realKey = "(Default)";
                if (key.Key != "") {
                    realKey = key.Key;
                }
                 ret += String.Format("{0}{1}: {2}\n", indent, realKey, key.Value);
            }

            foreach (var dirEl in SubDirectories) {
                ret += String.Format("{0}{1}\\ \n", indent, dirEl.Key);
                var dir = dirEl.Value;
                ret += dir.toStr(n + 1);
            }

            return ret;
        }

        /*
        public void GetKey()
        {

        }
        public void HasKey()
        {

        }
        public void AddKey()
        {

        }*/
    }

    [Serializable]
    public class _XmlAssociation
    {
        // For both AppAssoc.xml and OEMDefaultAssociations.xml
        public string Extension { get; set; } = "";
        public string Progid { get; set; } = "";
        public string AppName { get; set; } = "";

        // Only in OEMDefaultAssociations.xml
        public string NewBrowserProgId { get; set; } = "";
        public string ApplyOnUpgrade { get; set; } = "";
        public string OverwriteIfProgIdIs { get; set; } = "";
    }

}
