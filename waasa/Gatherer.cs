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

namespace waasa
{

    [Serializable]
    public class _GatheredData
    {
        public List<string> ListedExtensions { get; set; } = new List<string>();


        // HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\
        public _RegDirectory HKCU_ExplorerFileExts { get; set; }

        // HKCU\\SOFTWARE\Microsoft\Windows\CurrentVersion\ApplicationAssociationToasts
        public _RegDirectory HKCU_ApplicationAssociationToasts { get; set; }

        // HKCU\\Software\\Classes\\
        public _RegDirectory HKCU_SoftwareClasses { get; set; }


        // HKCR\\
        public _RegDirectory HKCR { get; set; }

        // HKCR\\SystemFileAssociations
        public _RegDirectory HKCR_SystemFileAssociations { get; set; }

        // HKCR\\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\Extensions\windows.fileTypeAssociation
        public _RegDirectory HKCR_FileTypeAssociations { get; set; }

        // HKCR\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository\
        public _RegDirectory HKCR_PackageRepository { get; set; }

        public Dictionary<string, Shlwapi.Assoc> ShlwapiAssoc { get; set; } = new Dictionary<string, Shlwapi.Assoc>();

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
            Console.WriteLine("  HKCR_PackageRepository               : " + HKCR_PackageRepository.SubDirectories.Count);
            Console.WriteLine("  ShlwapiAssoc                         : " + ShlwapiAssoc.Count);
        }

        public string GetShlwapiInfo(string extension)
        {
            return ShlwapiAssoc[extension].ToString();
        }


        // Gather human-readable information about a extension
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


        // Gather human-readable information about a objid
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


    class Gatherer
    {
        public _GatheredData GatheredData = new _GatheredData();

        public _GatheredData GatherAll()
        {
            Console.WriteLine("Gather system data");
            GatherListedExtensions();
            
            GatherRegistryHKCR();
            GatherRegistryHKCU_ExplorerFileExts();
            GatherRegistryHKCU_ApplicationAssociationToasts();
            GatherRegistryHKCU_SoftwareClasses();
            GatherHKCR_SystemFileAssociations();
            GatherHKCR_FileTypeAssociations();
            GatherHKCR_PackageRepository();
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
            HashSet<string> excludes = new HashSet<string>
                { "Interface", "Extensions", "Local Settings", "WOW6432Node", "Installer" ,  
                  "ActivatableClasses", "AppId", "CID", "Record", "TypeLib"};

            GatheredData.HKCR = FromRegistry(Microsoft.Win32.Registry.ClassesRoot, "", excludes);
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


        private void GatherHKCR_PackageRepository()
        {
            HashSet<string> excludes = new HashSet<string>() {
                "Extensions", // Really?
            };
            GatheredData.HKCR_PackageRepository = FromRegistry(Microsoft.Win32.Registry.ClassesRoot, @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\PackageRepository");

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

        private _RegDirectory FromRegistry(RegistryKey registryKey, string path, HashSet<string> excludes = null)
        {
            // Open initial directory
            _RegDirectory rootDir = new _RegDirectory(registryKey.Name + "\\" + path);
            var reg = registryKey.OpenSubKey(path);
            if (reg == null) {
                throw new InvalidOperationException("Registry key not found: " + registryKey.Name + "\\" + path);
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

                // Skip directories which are excluded (on root)
                if (excludes != null) {
                    if (excludes.Contains(fuck)) {
                        continue;
                    }
                }

                var dirRes = FromRegistry(registryKey, fuck);
                rootDir.AddDir(dirName, dirRes);
            }

            return rootDir;
        }
    }
}
