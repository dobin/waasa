using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;

using System.Text.Json;


namespace waasa
{
    public partial class MainWindow : Window
    {
        private _GatheredData GatheredData;
        private string DumpFilepath;
        private string OpensFilepath;
        private List<_FileExtension> FileExtensions;

        public MainWindow(string dumpFilepath, string opensFilepath)
        {
            InitializeComponent();
            DumpFilepath = dumpFilepath;
            OpensFilepath = opensFilepath;

            load();
            //dataGrid.ItemsSource = fileExtensions;
            //GatheredData = gatheredData;
        }

        private void load()
        {
            Console.WriteLine("Dump: " + DumpFilepath);
            if (!File.Exists(DumpFilepath)) {
                Console.WriteLine("  Not found. No data.");
                return;
            }
            string jsonString = File.ReadAllText(DumpFilepath);
            GatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            parseGatheredData();
        }

        private void parseGatheredData() { 
            var Validator = new Validator();
            Validator.LoadFromFile(OpensFilepath);

            var Registry = new VirtRegistry(GatheredData);
            var Analyzer = new Analyzer(GatheredData, Validator, Registry);
            FileExtensions = Analyzer.AnalyzeGatheredData();

            dataGrid.ItemsSource = FileExtensions;
        }


        private void ButtonExec(object sender, RoutedEventArgs e)
        {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;

            string filepath = System.Environment.GetEnvironmentVariable("TEMP") + "\\test" + fe.Extension;
            Console.WriteLine($"File to exec: {filepath}");

            File.Create(filepath);
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
            startInfo.Arguments = filepath;
            Process.Start(startInfo);
        }


        private void ButtonDownload(object sender, RoutedEventArgs e)
        {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
        }

        private void ButtonBrowserDownload(object sender, RoutedEventArgs e)
        {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
        }

        private void ButtonDetails(object sender, RoutedEventArgs e)
        {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            WindowInfo secondWindow = new WindowInfo(GatheredData, fe);
            secondWindow.Show();
        }

        private void Menu_Scan(object sender, RoutedEventArgs e)
        {
            var gather = new Gatherer();
            GatheredData = gather.GatherAll();
            parseGatheredData();
        }
        private void Menu_SaveCsv(object sender, RoutedEventArgs e)
        {
            var Registry = new VirtRegistry(GatheredData);
            AppSharedFunctionality.handleCsvDebug("output.csv", FileExtensions, Registry);
        }
        private void Menu_CreateFiles(object sender, RoutedEventArgs e)
        {
            AppSharedFunctionality.handleFiles(FileExtensions);
        }
        private void Menu_LoadFile(object sender, RoutedEventArgs e)
        {
        }
    }
}
