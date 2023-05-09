using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

            var x = gatheredData.HKCR.GetDir(objid);
            x.Print(1);

        }

        public void WriteCsv(List<_FileExtension> fileExtensions)
        {
            using (StreamWriter writer = new StreamWriter("output.csv")) {
                foreach (var fileExtension in fileExtensions) {
                    writer.WriteLine(String.Format("{0};{1};{2}", fileExtension.Extension, fileExtension.Result, fileExtension.Assumption));
                }
            }
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

        public void dumpToJson(_GatheredData gatheredData)
        {
            using (StreamWriter writer = new StreamWriter("dump.json")) {
                string strJson = JsonSerializer.Serialize<_GatheredData>(gatheredData);
                writer.WriteLine(strJson);
            }
        }
    }
}
