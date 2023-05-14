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


namespace waasa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        _GatheredData loadData(string filename)
        {
            Console.WriteLine("Dump: " + filename);
            string jsonString = File.ReadAllText(filename);
            var gatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            //gatheredData.PrintStats();
            return gatheredData;
        }

        Validator loadValidator(string filepath)
        {
            Validator validator = new Validator();
            validator.Load(filepath);
            return validator;
        }


        void handleCsv(_GatheredData gatheredData, Validator validator, string filepath)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            validator.Validate(fileExtensions);

            var output = new Output();
            output.WriteCsv(fileExtensions, filepath);
        }


        void handleGui(_GatheredData gatheredData, Validator validator)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            validator.Validate(fileExtensions);

            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow mainWindow = new MainWindow(gatheredData, fileExtensions);
            mainWindow.Show();
        }


        void testOne(_GatheredData gatheredData, Validator validator, string extension)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeSingle(extension);
            var fileExtensions = analyze.FileExtensions;

            validator.Validate(fileExtensions);

            var output = new Output();
            output.printCsv(fileExtensions);
        }


        void handleCsvDebug(_GatheredData gatheredData, Validator validator, string filepath)
        {
            var analyze = new Analyze(gatheredData);
            var debugEntries = analyze.GetDebug();

            validator.ValidateDebug(debugEntries);

            var output = new Output();
            output.WriteCsvDebug(debugEntries, filepath);
        }


        void testAll(_GatheredData gatheredData, Validator validator)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            validator.Validate(fileExtensions);
            validator.PrintStats(fileExtensions);
        }


        void handleFiles(_GatheredData gatheredData, Validator validator)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            validator.Validate(fileExtensions);

            var output = new Output();
            output.WriteFiles(fileExtensions);
        }


        void handleExt(_GatheredData gatheredData, string extension)
        {
            var analyze = new Analyze(gatheredData);
            var output = new Output();
            output.WriteExts(gatheredData, extension);
        }


        void handleObjid(_GatheredData gatheredData, string objid, string extension)
        {
            var analyze = new Analyze(gatheredData);
            var output = new Output();
            output.WriteObjid(gatheredData, objid);

            Console.WriteLine("");
            Console.WriteLine("Toast: " + analyze.hasToast(extension, objid));

        }

        void handleAssoc(_GatheredData gatheredData, string data)
        {
            var analyze = new Analyze(gatheredData);
            var a = analyze.GetShlwapiBy(data);
            Console.WriteLine("Assoc:\n" + a.ToString());
        }


        void dumpToJson(string filepath)
        {
            Console.WriteLine("Gathering all data from current system");
            var gather = new Gather();
            gather.GatherAll();
            var gatheredData = gather.GatheredData;

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
            [Option("ext", Required = false, HelpText = "")]
            public string InfoExt { get; set; }

            [Option("obj", Required = false, HelpText = "")]
            public string InfoObjid { get; set; }

            [Option("assoc", Required = false, HelpText = "")]
            public string Assoc { get; set; }


            // Generate
            [Option("files", Required = false, HelpText = "")]
            public bool Files { get; set; }

            // Other
            [Option("gui", Required = false, HelpText = "")]
            public bool Gui { get; set; }

            // Other
            [Option("extinfo", Required = false, HelpText = "")]
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
                 if (o.Dump != null) {
                     dumpToJson(o.Dump);
                     return;
                 }

                 _GatheredData gatheredData = loadData(o.DumpInputFile);
                 var validator = loadValidator(o.OpensInputFile);
                 Console.WriteLine("");
                 if (o.TestOne != null) {
                     testOne(gatheredData, validator, o.TestOne);
                 } else if (o.Gui) {
                     handleGui(gatheredData, validator);
                     return;
                 } else if (o.TestAll) {
                     testAll(gatheredData, validator);
                 } else if (o.Csv != null) {
                     handleCsv(gatheredData, validator, o.Csv);
                 } else if (o.CsvDebug != null) {
                     handleCsvDebug(gatheredData, validator, o.CsvDebug);
                 } else if (o.Files) {
                     handleFiles(gatheredData, validator);
                 } else if (o.InfoExt != null) {
                     handleExt(gatheredData, o.InfoExt);
                 } else if (o.InfoObjid != null) {
                     handleObjid(gatheredData, o.InfoObjid, o.Ext);
                 } else if (o.Assoc != null) {
                     handleAssoc(gatheredData, o.Assoc);
                 }
             });

            // Shutdown the application when done
            //this.Shutdown();
        }
    }
}
