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
using System.Threading.Tasks;
using Serilog;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;

namespace waasa {

    /// <summary>
    /// The main class
    /// </summary>
    public partial class App : Application {

        static void commandDump(string file, string csv) {
            if (csv != null) {
                Log.Information("Dump to csv: " + csv);
                var fileExtensions = Io.DumpFromSystem();
                Io.WriteResultsToCsv(fileExtensions, csv);
            } else if (file != null) { // Default
                Log.Information("Dump to json: " + file);
                var fileExtensions = Io.DumpFromSystem();
                Io.WriteResultJson(fileExtensions, file);
            } else {
                Log.Information("Dump to json: Error, specify --csv or --file");
            }
        }


        static async void commandContentfilter(string file, bool all, string manual) {
            if (all) {
                Log.Information("cfCommand: all");
                var fileExtensions = Io.DumpFromSystem();
                await ContentFilter.analyzeExtensions(fileExtensions);
            } else if (manual != null) {
                Log.Information("cfCommand: manual " + manual);
                var fileExtensions = Io.ReadManual(manual);
                await ContentFilter.analyzeExtensions(fileExtensions);
            } else if (file != null) {
                Log.Information("cfCommand: results " + file);
                var fileExtensions = Io.ReadResultJson(file);
                await ContentFilter.analyzeExtensions(fileExtensions);
            } else {
                Log.Information("cfCommand: Error");
            }
        }


        static void commandGenfiles(string file, bool all, string manual) {
            if (all) {
                Log.Information("genFilesCommand: all");
                var fileExtensions = Io.DumpFromSystem();
                Io.GenerateFiles(fileExtensions);
            } else if (manual != null) {
                Log.Information("genFilesCommand: manual " + manual);
                var fileExtensions = Io.ReadManual(file);
                Io.GenerateFiles(fileExtensions);
            } else if (file != null) {
                Log.Information("genFilesCommand: results " + file);
                var fileExtensions = Io.ReadResultJson(file);
                Io.GenerateFiles(fileExtensions);
            } else {
                Log.Information("genFilesCommand: Error");
            }
        }

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
        */


        protected override async void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Information("waasa - Windows Application Attack Surface Analyzer");
            // Set the shutdown mode to explicit shutdown
            //this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var rootCommand = new RootCommand("waasa - Windows Application Attack Surface Analyzer");

            // Dump (input: system, results. output: results, csv)
            var fileOption = new Option<string?>(
                 name: "--file",
                 description: "(waasa.json)", 
                 getDefaultValue: () => "waasa.json");
            var csvOption = new Option<string?>(
                 name: "--csv",
                 description: "(waasa.csv)");
            var dumpCommand = new Command("dump", "Dump to the waasa JSON output filename") {
                fileOption,
                csvOption
            };
            dumpCommand.SetHandler((file, csv) => {
                commandDump(file!, csv!);
            }, fileOption, csvOption);
            
            rootCommand.AddCommand(dumpCommand);

            // Content Filter (input: local, results, manual. output: corrensponding)
            var allOption = new Option<bool>(
                 name: "--all",
                 description: "All from local system (can take a long time)");
            fileOption = new Option<string?>(
                 name: "--file",
                 description: "File to read extensions from (waasa.json)");
            var manualOption = new Option<string?>(
                 name: "--manual",
                 description: "(waasa.txt)");
            var cfCommand = new Command("contentfilter", "Perform content filter tests") {
                allOption,
                fileOption,
                manualOption
            };
            cfCommand.SetHandler(async (file, all, manual) => {
                commandContentfilter(file!, all!, manual!);
            }, fileOption, allOption, manualOption);
            rootCommand.AddCommand(cfCommand);

            // Generate all files (input: local, results. output: directory with files)
            allOption = new Option<bool>(
                 name: "--all",
                 description: "");
            fileOption = new Option<string?>(
                 name: "--file",
                 description: "(waasa.json)");
            manualOption = new Option<string?>(
                 name: "--manual",
                 description: "(waasa.txt)");
            var genfilesCommand = new Command("genfiles", "Debug: Generate all files") {
                fileOption,
                allOption,
                manualOption
            };
            genfilesCommand.SetHandler(async (file, all, manual) => {
                commandGenfiles(file!, all!, manual!);
            }, fileOption, allOption, manualOption);
            rootCommand.AddCommand(genfilesCommand);

            string[] args = Environment.GetCommandLineArgs();
            args = args.Skip(1).ToArray();

            if (args.Length == 0) {
                // No arguments, start GUI
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                return;
            } else {
                await rootCommand.InvokeAsync(args);
                this.Shutdown();
            }
        }
    }
}
