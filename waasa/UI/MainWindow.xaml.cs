using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;

using System.Text.Json;
using System.ComponentModel;
using waasa.Services;
using waasa.Models;


namespace waasa {
    /// UI mostly done with ChatGPT
    /// 
    /// <summary>
    /// Main Window of the UI app
    /// </summary>
    public partial class MainWindow : Window {
        private _GatheredData GatheredData;
        private VirtRegistry Registry = new VirtRegistry();
        private List<_FileExtension> FileExtensions;

        ICollectionView collectionView;
        private string searchFilter = "";


        public MainWindow(string dumpFilepath, string opensFilepath) {
            InitializeComponent();
            DataContext = new MyViewModel(this);
            load(dumpFilepath, opensFilepath);
        }


        private void load(string dumpFilepath, string opensFilepath) {
            Console.WriteLine("Using waasa result data JSON file: " + dumpFilepath);

            // Should not happen
            if (!File.Exists(dumpFilepath)) {
                Console.WriteLine("  Not found, so no data");
                return;
            }

            // Load all data
            string jsonString = File.ReadAllText(dumpFilepath);
            GatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            var validator = new Validator();
            var analyzer = new Analyzer();
            Registry.Load(GatheredData);
            validator.LoadFromFile(opensFilepath);
            analyzer.Load(GatheredData, validator, Registry);
            FileExtensions = analyzer.AnalyzeGatheredData();

            // Connect the UI table with our data
            dataGrid.ItemsSource = FileExtensions;

            // Make table filterable
            collectionView = CollectionViewSource.GetDefaultView(FileExtensions);
            collectionView.Filter = obj => {
                if (obj is _FileExtension fileExtension) {
                    if (searchFilter == "") {
                        return true;
                    }
                    if (fileExtension.Extension.ToLower().Contains(searchFilter.ToLower())) {
                        return true;
                    }
                    if (fileExtension.AppPath.ToLower().Contains(searchFilter.ToLower())) {
                        return true;
                    }
                }
                return false;
            };
        }


        /*** Buttons ***/ 

        private void ButtonExec(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;

            string filepath = System.Environment.GetEnvironmentVariable("TEMP") + "\\test" + fe.Extension;
            Console.WriteLine($"File to exec: {filepath}");

            File.Create(filepath);
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
            startInfo.Arguments = filepath;
            Process.Start(startInfo);
        }

        private void ButtonDownload(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
        }

        private void ButtonBrowserDownload(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
        }

        private void ButtonDetails(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)(sender as Button).DataContext;
            WindowInfo secondWindow = new WindowInfo(GatheredData, fe);
            secondWindow.Show();
        }


        /*** Menu Items ***/

        private void Menu_DumpData(object sender, RoutedEventArgs e) {
            //var gather = new Gatherer();
            //GatheredData = gather.GatherAll();
            //parseGatheredData();
        }

        private void Menu_Filter(object sender, RoutedEventArgs e) {
            SetSearchFilter();
        }

        private void Menu_SaveCsv(object sender, RoutedEventArgs e) {
            AppSharedFunctionality.usageCreateResultsCsvDebug("output.csv", FileExtensions, Registry);
        }

        private void Menu_CreateFiles(object sender, RoutedEventArgs e) {
            AppSharedFunctionality.usageCreateTestFiles(FileExtensions);
        }

        private void Menu_LoadDumpFile(object sender, RoutedEventArgs e) {
        }

        
        /*** Other UI Functionality ***/

        public void SetSearchFilter() {
            WindowSearch windowSearch = new WindowSearch();
            if (windowSearch.ShowDialog() == true) {
                searchFilter = windowSearch.UserInput;
                collectionView.Refresh();
            }
        }
    }


    public class MyViewModel : INotifyPropertyChanged {
        MainWindow mywindow;

        public MyViewModel(MainWindow mainwindow) : base() {

            mywindow = mainwindow;
        }

        private ICommand searchCommand;
        public event PropertyChangedEventHandler? PropertyChanged;
        public ICommand SearchCommand {
            get {
                if (searchCommand == null) {
                    searchCommand = new RelayCommand(
                        param => this.Search(),
                        param => this.CanSearch()
                    );
                }
                return searchCommand;
            }
        }

        private void Search() {
            mywindow.SetSearchFilter();
        }

        private bool CanSearch() {
            return true;
        }
    }


    public class RelayCommand : ICommand {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null) {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter) {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
