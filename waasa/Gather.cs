using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Security.AccessControl;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;
using System.IO;
//using YamlDotNet.Serialization;
using System.Xml;

namespace waasa
{

    [Serializable]
    public class _GatheredData
    {
        public List<string> ListedExtensions { get; set; } = new List<string>();

        public _RegDirectory HKCR { get; set; }
        public _RegDirectory HKCU_ExplorerFileExts { get; set; }
        public _RegDirectory HKCU_ApplicationAssociationToasts { get; set; }
        public _RegDirectory HKCU_SoftwareClasses { get; set; }

        public _RegDirectory HKCR_SystemFileAssociations { get; set; }
        public _RegDirectory HKCR_FileTypeAssociations { get; set; }

        public Dictionary<string, Shlwapi.Assoc> ShlwapiAssoc { get; set; } = new Dictionary<string, Shlwapi.Assoc>();

        public List<_XmlAssociation> AppAssocXml { get; set; } = new List<_XmlAssociation>();
        public List<_XmlAssociation> DefaultAssocXml { get; set; } = new List<_XmlAssociation>();

        public void PrintStats()
        {
            Console.WriteLine("GatheredData:");
            Console.WriteLine("  ListedExtensions:                    : " + ListedExtensions.Count);
            Console.WriteLine("  HKCR                                 : " + HKCR.SubDirectories.Count);
            Console.WriteLine("  HKCU_ExplorerFileExts                : " + HKCU_ExplorerFileExts.SubDirectories.Count);
            Console.WriteLine("  HKCU_ApplicationAssociationToasts    : " + HKCU_ApplicationAssociationToasts.Keys.Count);
            Console.WriteLine("  HKCU_SoftwareClasses                 : " + HKCU_SoftwareClasses.Keys.Count);
            Console.WriteLine("  HKCR_SystemFileAssociations          : " + HKCR_SystemFileAssociations.SubDirectories.Count);
            Console.WriteLine("  HKCR_FileTypeAssociations            : " + HKCR_FileTypeAssociations.SubDirectories.Count);
            Console.WriteLine("  AppAssocXml                          : " + AppAssocXml.Count);
            Console.WriteLine("  DefaultAssocXml                      : " + DefaultAssocXml.Count);
            Console.WriteLine("  ShlwapiAssoc                         : " + ShlwapiAssoc.Count);
        }

        public string GetShlwapiInfo(string extension)
        {
            return ShlwapiAssoc[extension].ToString();
        }

        public string GetExtensionInfo(string extension)
        {
            string ret = "";

            ret += String.Format("Extension: {0}\n", extension);
            ret += String.Format("\n");

            // HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\
            ret += String.Format("HKLU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\ \n", extension);
            if (HKCU_ExplorerFileExts.HasDir(extension)) {
                var y = HKCU_ExplorerFileExts.GetDir(extension);
                ret += y.toStr(1);
            } else {
                ret += ("  NOT EXIST\n");
            }
            ret += String.Format("\n");

            // HKCR\
            ret += String.Format("HKCR\\{0}\\ \n", extension);
            if (HKCR.HasDir(extension)) {
                var x = HKCR.GetDir(extension);
                ret += x.toStr(1);
            } else {
                ret += "  Not found\n";
            }
            ret += String.Format("\n");

            // HKCU\Software\Classes
            ret += String.Format("\n");
            if (!HKCU_SoftwareClasses.HasDir(extension)) {
                ret += String.Format("HKCU\\Software\\Classes\\{0} not found\n", extension);
            } else {
                var x = HKCU_SoftwareClasses.GetDir(extension);
                ret += String.Format("HKCU\\Software\\Classes\\{0}\\ \n", extension);
                ret += x.toStr(1);
            }

            return ret;
        }


        public string GetObjidInfo(string objid)
        {
            string ret = "";

            // HKCR
            ret += String.Format("HKCR\\{0}\\ \n", objid);
            if (!HKCR.HasDir(objid)) {
                ret += String.Format("  not found\n", objid);
            } else {
                var x = HKCR.GetDir(objid);
                ret += x.toStr(1);
            }

            // HKCU\Software\Classes
            ret += String.Format("\n");
            ret += String.Format("HKCU\\Software\\Classes\\{0}\\ \n", objid);
            if (!HKCU_SoftwareClasses.HasDir(objid)) {
                ret += "  Not found";
            } else {
                var x = HKCU_SoftwareClasses.GetDir(objid);
                ret += x.toStr(1);
            }

            return ret;
        }
    }


    class Gather
    {
        public _GatheredData GatheredData = new _GatheredData();

        public _GatheredData GatherAll()
        {
            GatherListedExtensions();

            GatherRegistryHKCR();
            GatherRegistryHKCU_ExplorerFileExts();
            GatherRegistryHKCU_ApplicationAssociationToasts();
            GatherRegistryHKCU_SoftwareClasses();

            GatherHKCR_SystemFileAssociations();
            GatherHKCR_FileTypeAssociations();

            GatherAppAssocXml();
            GatherDefaultAssocXml();

            GatherShlwapi();

            GatheredData.PrintStats();

            return GatheredData;
        }

        public void GatherShlwapi()
        {
            foreach(var ext in GatheredData.ListedExtensions) {
                var assoc = Shlwapi.Query(ext);
                GatheredData.ShlwapiAssoc.Add(ext, assoc);
            }
        }

        public void GatherListedExtensions()
        {
            foreach (var key in Microsoft.Win32.Registry.ClassesRoot.GetSubKeyNames()) {
                if (key.StartsWith(".")) {
                    GatheredData.ListedExtensions.Add(key);
                }
            }
        }


        public void GatherRegistryHKCR()
        {
            Console.WriteLine("GatherRegistryHKCR");
            GatheredData.HKCR = FromRegistry(Microsoft.Win32.Registry.ClassesRoot, "");
            Console.WriteLine("  Finished");
        }

        public void GatherHKCR_SystemFileAssociations()
        {
            Console.WriteLine("GatherHKCR_SystemFileAssociations");
            GatheredData.HKCR_SystemFileAssociations = FromRegistry(Microsoft.Win32.Registry.ClassesRoot, @"SystemFileAssociations");
            Console.WriteLine("  Finished: " + GatheredData.HKCR.SubDirectories.Count);
        }
        public void GatherHKCR_FileTypeAssociations()
        {
            Console.WriteLine("GatherHKCR_FileTypeAssociations");
            GatheredData.HKCR_FileTypeAssociations = FromRegistry(Microsoft.Win32.Registry.ClassesRoot, @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Extensions\windows.fileTypeAssociation");
            Console.WriteLine("  Finished");
        }

        public void GatherRegistryHKCU_SoftwareClasses()
        {
            Console.WriteLine("GatherRegistryHKCU_SoftwareClasses");
            GatheredData.HKCU_SoftwareClasses = FromRegistry(Microsoft.Win32.Registry.CurrentUser, @"SOFTWARE\Classes");
            Console.WriteLine("  Finished");
        }


        public void GatherRegistryHKCU_ExplorerFileExts()
        {
            Console.WriteLine("GatherRegistryHKCU_ExplorerFileExts");
            GatheredData.HKCU_ExplorerFileExts = FromRegistry(Microsoft.Win32.Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts");
            Console.WriteLine("  Finished");
        }

        public void GatherRegistryHKCU_ApplicationAssociationToasts()
        {
            Console.WriteLine("GatherRegistryHKCU_ApplicationAssociationToasts");
            GatheredData.HKCU_ApplicationAssociationToasts = FromRegistry(Microsoft.Win32.Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts");
            Console.WriteLine("  Finished");
        }

        public void GatherAppAssocXml()
        {
            GatheredData.AppAssocXml = FromXml(@"AppAssoc.xml");
        }

        public void GatherDefaultAssocXml()
        {
            GatheredData.DefaultAssocXml = FromXml("C:\\Windows\\System32\\OEMDefaultAssociations.xml");
        }

        private _RegDirectory FromRegistry(RegistryKey registryKey, string path)
        {
            //Console.WriteLine("FromRegistry: " + path);

            _RegDirectory rootDir = new _RegDirectory(registryKey.Name + "\\" + path);

            var reg = registryKey.OpenSubKey(path);
            if (reg == null) {
                throw new InvalidOperationException("Registry key not found: " + registryKey.Name + "\\" + path);
                //Console.WriteLine("Registry key not found: " + registryKey.Name + " with path: " + path);
                return rootDir;
            }

            // Keys
            foreach (var key in reg.GetValueNames()) {
                var value = Convert.ToString(reg.GetValue(key));
                rootDir.Keys.Add(key, value);
            }

            // Directories
            foreach (var dirName in reg.GetSubKeyNames()) {
                var fuck = "";
                if (path == "") {
                    fuck = dirName;
                } else {
                    fuck = path + "\\" + dirName;
                }

                // Skip unecessary directories
                if (path != "Interface" && path != "Extensions" && path != "Local Settings" 
                    && path != "WOW6432Node" && path != "Installer" && path != "ActivatableClasses"
                    && path != "AppId" && path != "CID" && path != "Record" && path != "TypeLib" 
                ) {
                    var dirRes = FromRegistry(registryKey, fuck);
                    //rootDir.SubDirectories.Add(dirName, dirRes);
                    rootDir.AddDir(dirName, dirRes);
                }
            }

            return rootDir;
        }

        private List<_XmlAssociation> FromXml(string filepath)
        {
            List<_XmlAssociation> xmls = new List<_XmlAssociation>();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (var fileStream = File.OpenText(filepath))
            using (XmlReader reader = XmlReader.Create(fileStream, settings)) {
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Element:
                            if (reader.Name != "Association") {
                                continue;
                            }

                            _XmlAssociation xml = new _XmlAssociation();
                            xml.Extension = reader.GetAttribute("Identifier");
                            xml.Progid = reader.GetAttribute("ProgId");
                            xml.AppName = reader.GetAttribute("ApplicationName");

                            xml.NewBrowserProgId = reader.GetAttribute("NewBrowserProgId");
                            xml.ApplyOnUpgrade = reader.GetAttribute("ApplyOnUpgrade");
                            xml.OverwriteIfProgIdIs = reader.GetAttribute("OverwriteIfProgIdIs");

                            xmls.Add(xml);

                            break;
                    }
                }
            }

            return xmls;
        }

    }
}
