using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.IO;
using Serilog;
using System.ComponentModel;
using System.Threading.Tasks;

using waasa.Services;
using waasa.Models;
using waasa.UI;


namespace waasa {
    /// UI mostly done with ChatGPT
    /// 
    /// <summary>
    /// Main Window of the UI app
    /// </summary>
    public partial class MainWindow : Window {
        // Data
        private List<_FileExtension> FileExtensions = new List<_FileExtension>();

        // UI
        ICollectionView collectionView;
        private string searchFilter = "";
        private bool showReference = false;
        private string StatusBarText = "asdfasd";


        public MainWindow() {
            InitializeComponent();
            DataContext = new MyViewModel(this);
            UpdateStatus("Ready");

            var gatheredData = Io.ReadGatheredData("gathered_data.json");
            if (gatheredData != null) {
                var SimpleRegistryView = new GatheredDataSimpleView(gatheredData);
                var validator = new Validator();
                var analyzer = new Analyzer();
                analyzer.Load(gatheredData, validator, SimpleRegistryView);
                FileExtensions = analyzer.getResolvedFileExtensions();
                UpdateStatus("File with gathered data: gathered_data.json");
            } else {
                // Check if waasa-results.json exists, and load it if it does
                FileExtensions = Io.ReadResultJson("waasa-results.json");
                UpdateStatus("File with results: waasa-results.json");
            }

            MyInit();
        }

        private void UpdateStatus(string newStatus) {
            statusTextBlock.Text = newStatus;
        }


        private void MyInit() {
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


        /*** Menu Items ***/

        private void Menu_DumpData(object sender, RoutedEventArgs e) {
            // dump registry from localhost and load into the ui
            Log.Information("Dump from localhost");
            var progressBarWindow = new WindowProgress();
            
            // Background thread
            Task.Run(() =>
            {
                FileExtensions = Io.DumpFromSystem();
                this.Dispatcher.Invoke(() =>
                {
                    progressBarWindow.Close();
                });
            });

            progressBarWindow.ShowDialog(); // blocks until it is Close()-d
            UpdateStatus("Dump from local system, no file");
            MyInit();
        }


        private void Menu_LoadResultFile(object sender, RoutedEventArgs e) {
            // open a registry dump file and load into the UI
            Log.Information("Load dump file");
            
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "waasa-results";
            dialog.DefaultExt = ".json";
            dialog.Filter = "JSON files (.json)|*.json";

            bool? result = dialog.ShowDialog();
            if (result == false) {
                return;
            }

            string dumpFilepath = dialog.FileName;
            if (!File.Exists(dumpFilepath)) {
                return;
            }
            FileExtensions = Io.ReadResultJson(dumpFilepath);
            UpdateStatus("File with results: " + dumpFilepath);
            MyInit();
        }


        private void Menu_LoadManualFile(object sender, RoutedEventArgs e) {
            Log.Information("Load manual file");
            
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "manual";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text files (.txt)|*.txt";

            bool? result = dialog.ShowDialog();
            if (result == false) {
                return;
            }
            string filepath = dialog.FileName;
            FileExtensions = Io.ReadManual(filepath);
            UpdateStatus("File manual extensions: " + filepath);
            MyInit();
        }


        private void Menu_Filter(object sender, RoutedEventArgs e) {
            SetSearchFilter();
        }

        private void Menu_SaveJson(object sender, RoutedEventArgs e) {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "waasa";
            dialog.DefaultExt = ".json";
            dialog.Filter = "JSON files (.json)|*.json";

            bool? result = dialog.ShowDialog();
            if (result == false) {
                return;
            }
            string filepath = dialog.FileName;
            UpdateStatus("Results file " + filepath + " (saved)");

            Io.WriteResultJson(FileExtensions, filepath);
        }


        private void Menu_SaveCsv(object sender, RoutedEventArgs e) {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = "waasa";
            dialog.DefaultExt = ".csv";
            dialog.Filter = "CSV files (.csv)|*.csv";

            bool? result = dialog.ShowDialog();
            if (result == false) {
                return;
            }
            string filepath = dialog.FileName;
            Io.WriteResultsToCsv(FileExtensions, filepath);
            UpdateStatus("CSV File " + filepath + " (saved)");
        }

        private void Menu_CreateFiles(object sender, RoutedEventArgs e) {
            Io.GenerateFiles(FileExtensions);
        }


        private void MenuReferenceOnChecked(object sender, RoutedEventArgs e) {
            dataGrid.Columns[1].Visibility = Visibility.Visible;
        }

        private void MenuReferenceOnUnchecked(object sender, RoutedEventArgs e) {
            dataGrid.Columns[1].Visibility = Visibility.Collapsed;
        }


        /*** Row selection menu ***/

        private void menuExec(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                Io.ExecFile(fe.Extension);
            }
        }

        private void menuRegInfo(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                WindowInfo secondWindow = new WindowInfo(fe);
                secondWindow.Show();
            }
        }

        private async void menuCfDownload(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                await ContentFilter.analyzeExtension(fe);
                collectionView.Refresh();
            }
        }

        private void menuCfInfo(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                WindowDownload secondWindow = new WindowDownload(fe);
                secondWindow.Show();
            }
        }

        private void menuCfBrowser(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                Log.Information($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
            }
        }

        private void menuOpenDir(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                Io.OpenDir(fe.AppPath);
            }
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


    // I have no idea about this shit
#pragma warning disable CS8618
    public class MyViewModel : INotifyPropertyChanged {
        private MainWindow mywindow;
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

        public MyViewModel(MainWindow mainwindow) : base() {
            mywindow = mainwindow;
        }

        private void Search() {
            mywindow.SetSearchFilter();
        }

        private bool CanSearch() {
            return true;
        }
    }
#pragma warning restore CS8618, CS8625, CS8767, CS8612


    // I have no idea about this shit
#pragma warning disable CS8618, CS8625, CS8767, CS8612
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
#pragma warning restore CS8618, CS8625, CS8767, CS8612
}
