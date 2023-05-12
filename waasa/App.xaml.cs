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


namespace waasa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        private _GatheredData gatheredData;
        private Validator Validator;

        void loadData(string filename)
        {
            Console.WriteLine("Loading data from: " + filename);
            string jsonString = File.ReadAllText(filename);
            gatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            //gatheredData.PrintStats();
        }

        void loadValidator(string filepath)
        {
            Validator = new Validator();
            Validator.Load(filepath);
        }


        void handleCsv(string filepath)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            Validator.Validate(fileExtensions);

            var output = new Output();
            output.WriteCsv(fileExtensions, filepath);
        }


        void testOne(string extension)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeSingle(extension);
            var fileExtensions = analyze.FileExtensions;

            Validator.Validate(fileExtensions);

            var output = new Output();
            output.printCsv(fileExtensions);
        }


        void handleCsvDebug(string filepath)
        {
            var analyze = new Analyze(gatheredData);
            var debugEntries = analyze.GetDebug();

            Validator.ValidateDebug(debugEntries);

            var output = new Output();
            output.WriteCsvDebug(debugEntries, filepath);
        }


        void testAll()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            Validator.Validate(fileExtensions);
            Validator.PrintStats(fileExtensions);
        }


        void handleFiles()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            Validator.Validate(fileExtensions);

            var output = new Output();
            output.WriteFiles(fileExtensions);
        }


        void handleExt(string extension)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var output = new Output();
            output.WriteExts(gatheredData, extension);
        }


        void handleObjid(string objid, string extension)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var output = new Output();
            output.WriteObjid(gatheredData, objid);

            Console.WriteLine("");
            Console.WriteLine("Toast: " + analyze.hasToast(extension, objid));

        }


        void dumpToJson(string filepath)
        {
            Console.WriteLine("Gathering all data from current system");
            var gather = new Gather();
            gather.GatherAll();
            gatheredData = gather.GatheredData;

            var output = new Output();
            output.dumpToJson(gatheredData, filepath);
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
            [Option("infoext", Required = false, HelpText = "")]
            public string InfoExt { get; set; }

            [Option("infoobjid", Required = false, HelpText = "")]
            public string InfoObjid { get; set; }

            // Generate
            [Option("files", Required = false, HelpText = "")]
            public bool Files { get; set; }

            // Other
            [Option("gui", Required = false, HelpText = "")]
            public bool Gui { get; set; }

            // Other
            [Option("ext", Required = false, HelpText = "")]
            public string Ext { get; set; }
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Set the shutdown mode to explicit shutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            CommandLine.Parser.Default.ParseArguments<Options>(e.Args)
              .WithParsed<Options>(o =>
             {
                 if (o.Gui) {
                     this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                     MainWindow mainWindow = new MainWindow();
                     mainWindow.Show();
                     return;
                 } else if (o.Dump != null) {
                     dumpToJson(o.Dump);
                     return;
                 }

                 loadData(o.DumpInputFile);
                 loadValidator(o.OpensInputFile); // Currently load for all options

                 if (o.TestOne != null) {
                     testOne(o.TestOne);
                 } else if (o.TestAll) {
                     testAll();
                 } else if (o.Csv != null) {
                     handleCsv(o.Csv);
                 } else if (o.CsvDebug != null) {
                     handleCsvDebug(o.CsvDebug);
                 } else if (o.Files) {
                     handleFiles();
                 } else if (o.InfoExt != null) {
                     handleExt(o.InfoExt);
                 } else if (o.InfoObjid != null) {
                     handleObjid(o.InfoObjid, o.Ext);
                 }
             });

            // Shutdown the application when done
            this.Shutdown();
        }
    }
}
