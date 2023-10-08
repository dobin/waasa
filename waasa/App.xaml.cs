﻿using System;
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
using System.Threading.Tasks;
using Serilog;


namespace waasa {
  
    /// <summary>
    /// The main class
    /// </summary>
    public partial class App : Application {
        /*
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


        async Task UsageHttp(string ext) {
            Log.Information("Content Filter Test for extension {A}", ext);
            _FileExtension fe = new _FileExtension();
            fe.Extension = ext;
            await ContentFilter.analyzeExtension(fe);


            Console.WriteLine($"Default: {fe.TestResults[0].Conclusion} ({fe.TestResults[0].HttpStatusCode})");
            Console.WriteLine($"NoMime: {fe.TestResults[1].Conclusion} ({fe.TestResults[1].HttpStatusCode})");
            Console.WriteLine($"NoMimeNoFilename: {fe.TestResults[2].Conclusion} ({fe.TestResults[2].HttpStatusCode})");
        }
        */


        void FunctionalityDump(string filepath) {
            Log.Information("Gathering all data from current system");
            Log.Information("Filename for data: " + filepath);

            var fileExtensions = Io.DumpFromSystem();
            Io.WriteResultJson(fileExtensions, filepath);
        }


        void FunctionalityGui(string filepath) {
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }


        #pragma warning disable CS8618
        public class Options {
            [Option("dump", Required = false, HelpText = "Dump to the waasa JSON output filename")]
            public string Dump { get; set; }


            /*
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
            */
        }
        #pragma warning restore CS8618


        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Information("waasa - Windows Application Attack Surface Analyzer");
            // Set the shutdown mode to explicit shutdown
            //this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            CommandLine.Parser.Default.ParseArguments<Options>(e.Args)
              .WithParsed<Options>(async o => {
                  bool ui = false;

                  if (o.Dump != null) {
                      FunctionalityDump(o.Dump);
                  } else {
                      FunctionalityGui("waasa-results.json");
                      ui = true;
                  }

                  /*
                  if (o.TestAll) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      UsageTestAll();
                  } else if (o.TestOne != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      UsageTestOne(o.TestOne);
                  } else if (o.Csv != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateResultsCsv(o.Csv, fileExtensions);
                  } else if (o.CsvDebug != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateResultsCsvDebug(o.CsvDebug, fileExtensions, SimpleRegistryView);
                  } else if (o.Files) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      var fileExtensions = Analyzer.getResolvedFileExtensions();
                      AppSharedFunctionality.usageCreateTestFiles(fileExtensions);
                  } else if (o.InfoExt != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      UsageTestExt(o.InfoExt);
                  } else if (o.InfoObj != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      UsageTestObjid(o.InfoObj);
                  } else if (o.Winapi != null) {
                      initCmd(o.DumpInputFile, o.OpensInputFile);
                      UsageTestWinApi(o.Winapi);
                  } else if (o.Dump != null) {
                      UsageDumpDataToFile(o.Dump);
                  } else if (o.Http != null) {
                      await UsageHttp(o.Http);
                  } else {
                      initUi(o.DumpInputFile, o.OpensInputFile);
                      UsageGui();
                      ui = true;
                  }
                  */

                  if (!ui) {
                      this.Shutdown();
                  }
              });
        }
    }
}
