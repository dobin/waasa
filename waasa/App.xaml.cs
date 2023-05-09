using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace waasa
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        private _GatheredData gatheredData;

        void loadData(string filename)
        {
            if (filename == "") {
                Console.WriteLine("Gathering all data from current system");
                var gather = new Gather();
                gather.GatherAll();
                gatheredData = gather.GatheredData;
            } else {
                Console.WriteLine("Loading data from: " + filename);
                string jsonString = File.ReadAllText(filename);
                gatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
                gatheredData.PrintStats();
            }
        }

        void handleCsv()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var validator = new Validator();
            validator.Load(@"C:\Users\dobin\source\repos\AppSurface\AppSurface\tests\opens.txt");
            validator.Validate(fileExtensions);

            var output = new Output();
            output.WriteCsv(fileExtensions);
        }

        void handleTest()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var validator = new Validator();
            validator.Load(@"C:\Users\dobin\source\repos\AppSurface\AppSurface\tests\opens.txt");
            validator.Validate(fileExtensions);
            validator.PrintStats(fileExtensions);
        }

        void handleFiles()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var validator = new Validator();
            validator.Load(@"C:\Users\dobin\source\repos\AppSurface\AppSurface\tests\opens.txt");
            validator.Validate(fileExtensions);

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


        void handleObjid(string objid)
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var output = new Output();
            output.WriteObjid(gatheredData, objid);
        }

        void dumpToJson()
        {
            var analyze = new Analyze(gatheredData);
            analyze.AnalyzeAll();
            var fileExtensions = analyze.FileExtensions;

            var output = new Output();
            output.dumpToJson(gatheredData);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Set the shutdown mode to explicit shutdown
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var function = e.Args[0];
            var dumpjson = "dump.json";

            switch (function) {
                case "gui":
                    this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    break;
                case "csv":
                    loadData(dumpjson);
                    handleCsv();
                    break;
                case "test":
                    loadData(dumpjson);
                    handleTest();
                    break;
                case "files":
                    loadData(dumpjson);
                    handleFiles();
                    break;

                case "dump":
                    dumpToJson();
                    break;

                case "ext":
                    loadData(dumpjson);
                    string ext = e.Args[1];
                    handleExt(ext);
                    break;

                case "objid":
                    loadData(dumpjson);
                    string objid = e.Args[1];
                    handleObjid(objid);
                    break;
            }

            // Your console application logic goes here
            Console.WriteLine("Hello from WPF Console App!");

            // Shutdown the application when done
            this.Shutdown();
        }
    }
}
