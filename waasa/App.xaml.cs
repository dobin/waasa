using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using waasa.Services;
using waasa.Models;


namespace waasa {
    /// <summary>
    /// Functionality which is also used in the GUI
    /// </summary>
    class AppSharedFunctionality {
        static public void usageCreateResultsCsv(string filepath, List<_FileExtension> fileExtensions) {
            Console.WriteLine("Writing CSV to: " + filepath + " with " + fileExtensions.Count);

            List<_CsvEntry> csvEntries = new List<_CsvEntry>();
            foreach (var fileExtension in fileExtensions) {
                csvEntries.Add(new _CsvEntry(fileExtension));
            }

            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(csvEntries);
        }


        static public void usageCreateResultsCsvDebug(string filepath, List<_FileExtension> fileExtensions, GatheredDataSimpleView registry) {
            var fileExtensionsDebug = registry.GetFileExtensionDebug(fileExtensions);

            Console.WriteLine("Writing CSVDebug to: " + filepath + " with " + fileExtensionsDebug.Count);
            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(fileExtensionsDebug);
        }


        static public void usageCreateTestFiles(List<_FileExtension> fileExtensions) {
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
    }

    /// <summary>
    /// The main function?
    /// </summary>
    public partial class App : Application {
        private _GatheredData GatheredData { get; set; } = new _GatheredData();
        private GatheredDataSimpleView SimpleRegistryView { get; set; } = new GatheredDataSimpleView(new _GatheredData());
        private Validator Validator { get; set; } = new Validator();
        private Analyzer Analyzer { get; set; } = new Analyzer();


        /// <summary>
        /// Init everything required, from dumpFilePath and opensFilepath
        /// </summary>
        private void init(string dumpFilepath, string opensFilepath) {
            if (!File.Exists(dumpFilepath)) {
                Console.WriteLine("Dump file doesnt exist, creating: " + dumpFilepath);
                UsageDumpDataToFile(dumpFilepath);
            }
            Console.WriteLine("Using data from file: " + dumpFilepath);

            string jsonString = File.ReadAllText(dumpFilepath);
            GatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            SimpleRegistryView = new GatheredDataSimpleView(GatheredData);
            Validator.LoadFromFile(opensFilepath);
            Analyzer.Load(GatheredData, Validator, SimpleRegistryView);
        }


        void UsageGui() {
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow mainWindow = new MainWindow(GatheredData, SimpleRegistryView, Analyzer);
            mainWindow.Show();
        }


        void UsageTestAll() {
            var fileExtensions = Analyzer.getResolvedFileExtensions();
            Validator.PrintStats(fileExtensions);
        }


        void UsageTestOne(string extension) {
            var fileExtensions = Analyzer.getResolvedFileExtensions();
            foreach (var fileExtension in fileExtensions) {
                if (fileExtension.Extension == extension) {
                    Console.WriteLine(String.Format("{0};{1};{2}", fileExtension.Extension, fileExtension.Result, fileExtension.Assumption));
                }
            }
        }


        void UsageTestExt(string extension) {
            Console.WriteLine(GatheredData.GetExtensionInfo(extension));
        }


        void UsageTestObjid(string objid) {
            Console.WriteLine(GatheredData.GetObjidInfo(objid));
        }


        void UsageTestWinApi(string ext) {
            var a = GatheredData.WinapiData[ext];
            Console.WriteLine("Assoc:\n" + a.ToString());
        }


        void UsageDumpDataToFile(string filepath) {
            Console.WriteLine("Gathering all data from current system and store it in: " + filepath);
            var gather = new Gatherer();
            var gatheredData = gather.GatherAll();

            using (StreamWriter writer = new StreamWriter(filepath)) {
                string strJson = JsonSerializer.Serialize<_GatheredData>(gatheredData);
                writer.WriteLine(strJson);
            }
        }

        async void UsageHttp(string a) {
            ContentFilter cf = new ContentFilter();
            //await cf.analyze();
            //HttpAnswerInfo answer = await Requestor.Get("");
            //Console.WriteLine("StautsCode: " + answer.StatusCode);
            //Console.WriteLine("Filename CD: " + answer.Filename);
            //Console.WriteLine("Data: " + answer.Content);
        }

        #pragma warning disable CS8618
        public class Options {
            [Option("verbose", Required = false, HelpText = "More detailed output")]
            public bool Verbose { get; set; }

            // Input
            [Option("dumpfile", Required = false, Default = "waasa.json", HelpText = "Path to the dump file")]
            public string DumpInputFile { get; set; }

            [Option("opensfile", Required = false, Default = "opens.txt", HelpText = "Path to the opens.txt")]
            public string OpensInputFile { get; set; }

            // Output to file
            [Option("csv", Required = false, HelpText = "The output CSV filename")]
            public string Csv { get; set; }

            [Option("csvdebug", Required = false, HelpText = "The debug output CSV filename")]
            public string CsvDebug { get; set; }

            [Option("dump", Required = false, HelpText = "The waasa JSON output filename")]
            public string Dump { get; set; }

            // Testing
            [Option("testall", Required = false, HelpText = "")]
            public bool TestAll { get; set; }

            [Option("testone", Required = false, HelpText = "")]
            public string TestOne { get; set; }

            // Debug
            [Option("ext", Required = false, HelpText = "Print information about an extension from typical Registry keys")]
            public string InfoExt { get; set; }

            [Option("obj", Required = false, HelpText = "Print information about an ObjId from typical Registry keys")]
            public string InfoObj { get; set; }

            [Option("winapi", Required = false, HelpText = "Print information about an extension from Windows shlwAPI")]
            public string Winapi { get; set; }

            [Option("http", Required = false, HelpText = "")]
            public string Http { get; set; }


            // Generate
            [Option("files", Required = false, HelpText = "Generate a file of each extension into output/")]
            public bool Files { get; set; }
        }
        #pragma warning restore CS8618


        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            // Set the shutdown mode to explicit shutdown
            //this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            CommandLine.Parser.Default.ParseArguments<Options>(e.Args)
              .WithParsed<Options>(o => {
                  if (o.TestAll) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageTestAll();
                  } else if (o.TestOne != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageTestOne(o.TestOne);
                  } else if (o.Csv != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateResultsCsv(o.Csv, fileExtensions);
                  } else if (o.CsvDebug != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateResultsCsvDebug(o.CsvDebug, fileExtensions, SimpleRegistryView);
                  } else if (o.Files) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateTestFiles(fileExtensions);
                  } else if (o.InfoExt != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageTestExt(o.InfoExt);
                  } else if (o.InfoObj != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageTestObjid(o.InfoObj);
                  } else if (o.Winapi != null) {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageTestWinApi(o.Winapi);
                  } else if (o.Dump != null) {
                      UsageDumpDataToFile(o.Dump);
                  } else if (o.Http != null) {
                      UsageHttp(o.Http);
                  } else {
                      init(o.DumpInputFile, o.OpensInputFile);
                      UsageGui();
                  }
                  //this.Shutdown();
              });
        }
    }
}
