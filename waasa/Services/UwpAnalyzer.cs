using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Xml.Linq;
using Serilog;


namespace waasa.Services {
    public class UwpAnalyzer {
        public static string? progidToPath(string progId, _GatheredData gatheredData) {
            // Check if HKCR\<progId> exists
            if (gatheredData.HKCR.HasDir(progId)) {
                // take HKCR\<progId>\shell\open\packageId
                var id = gatheredData.HKCR.GetKey(progId + "\\shell\\open\\PackageId");
                if (id != "") {
                    // Use that to index into PackageRepository
                    if (gatheredData.HKCR_PackageRepository.HasDir(id)) {
                        var appPath = gatheredData.HKCR_PackageRepository.GetKey(id + "\\PackageRootFolder");
                        return appPath;
                    }
                }
            }
            return null;
        }

        public static string? parseManifest(string filePath) {
            XDocument xDoc;
            try {
                xDoc = XDocument.Load(filePath);
            } catch (Exception ex) {
                Log.Information("Error loading XML: " + ex.Message);
                return null;
            }

            XNamespace ns = xDoc.Root.GetDefaultNamespace();

            // Query the XML to find the Application element and get the Executable attribute
            /*var applicationElement = xDoc.Descendants(ns + "Application")
                                         .FirstOrDefault(a => (string)a.Attribute("Id") == "App");*/
            var applicationElement = xDoc.Descendants(ns + "Application").First();

            if (applicationElement != null) {
                string executable = (string)applicationElement.Attribute("Executable");
                return executable;
            } else {
                return null;
            }
        }
    }
}
