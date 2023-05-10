using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;


namespace waasa
{

    class Output
    {
        public void WriteExts(_GatheredData gatheredData, string extension) {
            Console.WriteLine("Extension: " + extension);
            Console.WriteLine();

            Console.WriteLine(@"HKLU\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\");
            var y = gatheredData.HKCU_ExplorerFileExts.GetDir(extension);
            y.Print(1);

            Console.WriteLine();
            Console.WriteLine(@"HKCR\");
            var x = gatheredData.HKCR.GetDir(extension);
            x.Print(1);
        }

        public void WriteObjid(_GatheredData gatheredData, string objid)
        {
            Console.WriteLine("Objid: " + objid);
            Console.WriteLine();

            // HKCR
            if (! gatheredData.HKCR.HasDir(objid)) {
                Console.WriteLine(String.Format("HKCR\\{0} not found", objid));
                return;
            }
            var x = gatheredData.HKCR.GetDir(objid);
            Console.WriteLine(@"HKCR\");
            x.Print(1);

            // Toast

            // Has Shell?
            // Has Exec?

        }

        public void printCsv(List<_FileExtension> fileExtensions)
        {
            foreach (var fileExtension in fileExtensions) {
                Console.WriteLine(String.Format("{0};{1};{2}", fileExtension.Extension, fileExtension.Result, fileExtension.Assumption));
            }
        }

        public void WriteCsv(List<_FileExtension> fileExtensions, string filepath)
        {
            using (StreamWriter writer = new StreamWriter(filepath)) {
                foreach (var fileExtension in fileExtensions) {
                    writer.WriteLine(String.Format("{0};{1};{2}", fileExtension.Extension, fileExtension.Result, fileExtension.Assumption));
                }
            }
        }

        public void WriteCsvDebug(List<_FileExtensionDebug> fileExtensionsDebug, string filepath)
        {
            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Configure CSV settings, e.g., delimiter, quoting, etc.
            };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(fileExtensionsDebug);
        }

        public void WriteFiles(List<_FileExtension> fileExtensions)
        {
            foreach (var app in fileExtensions) {
                var output = "output";
                var filename = "test" + app.Extension;
                var directory = app.Assumption;

                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(output + "\\" + directory)) {
                    Directory.CreateDirectory(output + "\\" + directory);
                }
                File.Create(output + "\\" + directory + "\\" + filename);
            }
        }

        public void dumpToJson(_GatheredData gatheredData, string filepath)
        {
            using (StreamWriter writer = new StreamWriter(filepath)) {
                string strJson = JsonSerializer.Serialize<_GatheredData>(gatheredData);
                writer.WriteLine(strJson);
            }
        }
    }
}
