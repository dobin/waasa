using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using System.Runtime.InteropServices;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace waasa
{
    public partial class App : Application
    {
        private _GatheredData GatheredData { get; set; }
        private _Registry Registry { get; set; }
        private Validator Validator { get; set; }
        private Analyze Analyzer { get; set; }


        private void loadAll(string dumpFilepath, string opensFilepath)
        {
            Console.WriteLine("Dump: " + dumpFilepath);
            string jsonString = File.ReadAllText(dumpFilepath);
            GatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;

            Validator = new Validator();
            Validator.Load(opensFilepath);

            Registry = new _Registry(GatheredData);
            Analyzer = new Analyze(GatheredData, Validator, Registry);
        }


        void handleGui()
        {
            var fileExtensions = Analyzer.AnalyzeAll();

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow mainWindow = new MainWindow(GatheredData, fileExtensions);
            mainWindow.Show();
        }


        void handleCsv(string filepath)
        {
            var fileExtensions = Analyzer.AnalyzeAll();

            Console.WriteLine("Writing CSV to: " + filepath + " with " + fileExtensions.Count);
            using (StreamWriter writer = new StreamWriter(filepath)) {
                foreach (var fileExtension in fileExtensions) {
                    writer.WriteLine(String.Format("{0};{1};{2}", fileExtension.Extension, fileExtension.Result, fileExtension.Assumption));
                }
            }
        }


        void handleCsvDebug(string filepath)
        {
            var fileExtensions = Analyzer.AnalyzeAll();
            var fileExtensionsDebug = Registry.GetFileExtensionDebug(fileExtensions);

            Console.WriteLine("Writing CSVDebug to: " + filepath + " with " + fileExtensionsDebug.Count);
            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {};
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(fileExtensionsDebug);
        }


        void testAll()
        {
            var fileExtensions = Analyzer.AnalyzeAll();
            Validator.Validate(fileExtensions);
            Validator.PrintStats(fileExtensions);
        }


        void handleFiles()
        {
            var fileExtensions = Analyzer.AnalyzeAll();
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


        void handleExt(string extension)
        {
            Console.WriteLine(GatheredData.GetExtensionInfo(extension));
        }


        void handleObjid(string objid)
        {
            Console.WriteLine(GatheredData.GetObjidInfo(objid));
        }


        void handleAssoc(string data)
        {
            var a = Analyzer.GetShlwapiBy(data);
            Console.WriteLine("Assoc:\n" + a.ToString());
        }


        void dumpToJson(string filepath)
        {
            Console.WriteLine("Gathering all data from current system");
            var gather = new Gather();
            var gatheredData = gather.GatherAll();

            using (StreamWriter writer = new StreamWriter(filepath)) {
                string strJson = JsonSerializer.Serialize<_GatheredData>(gatheredData);
                writer.WriteLine(strJson);
            }
        }


        public class Options
        {
            [Option("verbose", Required = false, HelpText = "More detailed output")]
            public bool Verbose { get; set; }

            // Input
            [Option("dumpfile", Required = false, Default = "dump.json", HelpText = "Path to the dump file")]
            public string DumpInputFile { get; set; }

            [Option("opensfile", Required = false, Default = "opens.txt", HelpText = "Path to the opens.txt")]
            public string OpensInputFile { get; set; }

            // Output to file
            [Option("csv", Required = false, HelpText = "")]
            public string Csv { get; set; }

            [Option("csvdebug", Required = false, HelpText = "")]
            public string CsvDebug { get; set; }

            [Option("dump", Required = false, HelpText = "")]
            public string Dump { get; set; }

            // Testing
            [Option("testall", Required = false, HelpText = "")]
            public bool TestAll { get; set; }

            [Option("testone", Required = false, HelpText = "")]
            public string TestOne { get; set; }

            // Debug
            [Option("ext", Required = false, HelpText = "")]
            public string InfoExt { get; set; }

            [Option("obj", Required = false, HelpText = "")]
            public string InfoObj { get; set; }

            [Option("assoc", Required = false, HelpText = "")]
            public string Assoc { get; set; }


            // Generate
            [Option("files", Required = false, HelpText = "")]
            public bool Files { get; set; }

            // Other
            [Option("gui", Required = false, HelpText = "")]
            public bool Gui { get; set; }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Set the shutdown mode to explicit shutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            /*
            var a = Shlwapi.Query(".androidproj");
            Console.WriteLine("COmmand: " + a.Command);
            Console.WriteLine("FriendlyAppName: " + a.FriendlyAppName);
            this.Shutdown();
            return;
            */
            

            CommandLine.Parser.Default.ParseArguments<Options>(e.Args)
              .WithParsed<Options>(o =>
             {
                 if (o.Dump != null) {
                     dumpToJson(o.Dump);
                     this.Shutdown();
                     return;
                 }

                 loadAll(o.DumpInputFile, o.OpensInputFile);

                 Console.WriteLine("");
                 if (o.Gui) {
                     handleGui();
                     return;
                 } else if (o.TestAll) {
                     testAll();
                     this.Shutdown();
                 } else if (o.Csv != null) {
                     handleCsv(o.Csv);
                     this.Shutdown();
                 } else if (o.CsvDebug != null) {
                     handleCsvDebug(o.CsvDebug);
                     this.Shutdown();
                 } else if (o.Files) {
                     handleFiles();
                     this.Shutdown();
                 } else if (o.InfoExt != null) {
                     handleExt(o.InfoExt);
                     this.Shutdown();
                 } else if (o.InfoObj != null) {
                     handleObjid(o.InfoObj);
                     this.Shutdown();
                 } else if (o.Assoc != null) {
                     handleAssoc(o.Assoc);
                     this.Shutdown();
                 }
             });
        }
    }
}
