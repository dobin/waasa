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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Pkcs;

namespace waasa {
    public class BoolToFriendlyStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool booleanValue) {
                return booleanValue ? "X" : "";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            // Implement if you need conversion back, otherwise throw NotSupportedException
            throw new NotSupportedException();
        }
    }

    public class ListToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is List<string> stringList) {
                return string.Join(", ", stringList);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


    /// UI mostly done with ChatGPT
    /// 
    /// <summary>
    /// Main Window of the UI app
    /// </summary>
    public partial class MainWindow : Window {
        // Main data strucutre, used for most of the UI
        private List<_FileExtension> FileExtensions = new List<_FileExtension>();

        // Optional, for some functionality
        private _GatheredData? gatheredData = null;

        // UI
        ICollectionView collectionView;
        private string searchFilter = "";
        private bool FilterRecommended = true;
        private bool FilterOpenwith = true;
        private bool FilterExec = true;
        private bool FilterUwp = true;
        private bool FilterWindows = true;
        private bool FilterPrograms = true;
        private bool FilterUser = true;


        public MainWindow() {
            InitializeComponent();
            DataContext = new MyViewModel(this);
            UpdateStatus("Ready");

            gatheredData = Io.ReadGatheredData("gathered_data.json");
            if (gatheredData != null) {
                var SimpleRegistryView = new GatheredDataSimpleView(gatheredData);
                var validator = new Validator();
                var analyzer = new Analyzer();
                analyzer.Load(gatheredData, validator, SimpleRegistryView);
                FileExtensions = analyzer.getResolvedFileExtensions();
                UpdateStatus("Autoloaded file with gathered data: gathered_data.json");
            } else {
                // Check if waasa-results.json exists, and load it if it does
                FileExtensions = Io.ReadResultJson("waasa-results.json");
                UpdateStatus("Autoloaded file with results: waasa-results.json");
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
                    if (! FilterRecommended && fileExtension.Assumption == "recommended") {
                        return false;
                    }
                    if (!FilterOpenwith && fileExtension.Assumption == "openwith") {
                        return false;
                    }
                    if (!FilterExec && fileExtension.Assumption == "exec") {
                        return false;
                    }
                    if (!FilterUwp && fileExtension.isUwp == true) {
                        return false;
                    }
                    if (!FilterWindows && fileExtension.CmdExePath.StartsWith("C:\\Windows\\")) {
                        return false;
                    }
                    if (!FilterPrograms && fileExtension.CmdExePath.StartsWith("C:\\Program Files")) {
                        return false;
                    }
                    if (!FilterUser && fileExtension.CmdExePath.StartsWith("C:\\Users\\")) {
                        return false;
                    }

                    // User input search filter
                    if (searchFilter == "") {
                        return true;
                    } else {
                        if (fileExtension.Extension.ToLower().Contains(searchFilter.ToLower())) {
                            return true;
                        }
                        if (fileExtension.CmdLine.ToLower().Contains(searchFilter.ToLower())) {
                            return true;
                        }
                        return false;
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
            var readFileExtensions = Io.ReadManual(filepath);
            if (FileExtensions.Count > 0) {
                var x = FileExtensions.Where(
                    feA => readFileExtensions.Any(feB => feB.Extension == feA.Extension));
                FileExtensions = x.ToList();
                UpdateStatus("File manual extensions " + filepath + " used to filter list");
            } else {
                FileExtensions = readFileExtensions;
                UpdateStatus("File manual extensions loaded: " + filepath);
            }

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


        /*** View ***/

        private void MenuReferenceOnChecked(object sender, RoutedEventArgs e) {
            if (dataGrid != null) {
                dataGrid.Columns[1].Visibility = Visibility.Visible;
            }
            
        }

        private void MenuReferenceOnUnchecked(object sender, RoutedEventArgs e) {
            if (dataGrid != null) {
                dataGrid.Columns[1].Visibility = Visibility.Collapsed;
            }
        }

        private void MenuViewExecOn(object sender, RoutedEventArgs e) {
            FilterExec = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuViewExecOff(object sender, RoutedEventArgs e) {
            FilterExec = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuViewERecommendedOn(object sender, RoutedEventArgs e) {
            FilterRecommended = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuViewRecommendedOff(object sender, RoutedEventArgs e) {
            FilterRecommended = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }


        private void MenuViewOpenwithOn(object sender, RoutedEventArgs e) {
            FilterOpenwith = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuViewOpenwithOff(object sender, RoutedEventArgs e) {
            FilterOpenwith = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuUwpOnChecked(object sender, RoutedEventArgs e) {
            FilterUwp = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuUwpOnUnchecked(object sender, RoutedEventArgs e) {
            FilterUwp = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuWindowsOnChecked(object sender, RoutedEventArgs e) {
            FilterWindows = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuWindowsOnUnchecked(object sender, RoutedEventArgs e) {
            FilterWindows = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuProgramsOnChecked(object sender, RoutedEventArgs e) {
            FilterPrograms = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuProgramsOnUnchecked(object sender, RoutedEventArgs e) {
            FilterPrograms = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuUserOnChecked(object sender, RoutedEventArgs e) {
            FilterUser = true;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }

        private void MenuUserOnUnchecked(object sender, RoutedEventArgs e) {
            FilterUser = false;
            if (collectionView != null) {
                collectionView.Refresh();
            }
        }


        /*** Row selection menu ***/

        private void menuExec(object sender, RoutedEventArgs e) {
            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                Io.ExecFile(fe.Extension);
            }
        }

        private void menuRegInfo(object sender, RoutedEventArgs e) {
            if (gatheredData == null) {
                MessageBox.Show("No gathered data available, load a dump.");
                return;
            }

            foreach (var item in dataGrid.SelectedItems) {
                var fe = item as _FileExtension;
                WindowInfo secondWindow = new WindowInfo(gatheredData, fe);
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
                Io.OpenDir(fe.CmdLine);
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
