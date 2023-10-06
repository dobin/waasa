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
using waasa.UI;
using System.Threading.Tasks;

namespace waasa {
    /// UI mostly done with ChatGPT
    /// 
    /// <summary>
    /// Main Window of the UI app
    /// </summary>
    public partial class MainWindow : Window {
        private _GatheredData GatheredData;
        private GatheredDataSimpleView GatheredDataSimpleView;
        private List<_FileExtension> FileExtensions;

        ICollectionView collectionView;
        private string searchFilter = "";

        private bool showReference = false;


        public MainWindow(_GatheredData gatheredData, GatheredDataSimpleView simpleView, Analyzer analyzer) {
            InitializeComponent();
            DataContext = new MyViewModel(this);

            GatheredData = gatheredData;
            GatheredDataSimpleView = simpleView;
            FileExtensions = analyzer.getResolvedFileExtensions();

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

        /** Registry & File related **/

        private void ButtonExecFile(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)((Button)sender).DataContext;
            string filepath = System.Environment.GetEnvironmentVariable("TEMP") + "\\test" + fe.Extension;
            File.Create(filepath);
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
            startInfo.Arguments = filepath;
            Process.Start(startInfo);
        }


        private void ButtonShowRegistryInfo(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)((Button)sender).DataContext;
            WindowInfo secondWindow = new WindowInfo(GatheredData, fe);
            secondWindow.Show();
        }


        /** Content Filter related **/

        private async void ButtonDownload(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)((Button)sender).DataContext;
            await ContentFilter.analyzeExtension(fe);
            collectionView.Refresh();
        }

        private void ButtonDownloadInfo(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)((Button)sender).DataContext;
            WindowDownload secondWindow = new WindowDownload(fe);
            secondWindow.Show();
        }

        private void ButtonBrowserDownload(object sender, RoutedEventArgs e) {
            _FileExtension fe = (_FileExtension)((Button)sender).DataContext;
            Console.WriteLine($"ButtonExec{fe.Extension} {fe.Result} clicked the button!");
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
            AppSharedFunctionality.usageCreateResultsCsvDebug("output.csv", FileExtensions, GatheredDataSimpleView);
        }

        private void Menu_CreateFiles(object sender, RoutedEventArgs e) {
            AppSharedFunctionality.usageCreateTestFiles(FileExtensions);
        }

        private void Menu_LoadDumpFile(object sender, RoutedEventArgs e) {
        }

        private void MenuReferenceOnChecked(object sender, RoutedEventArgs e) {
            dataGrid.Columns[1].Visibility = Visibility.Visible;
        }

        private void MenuReferenceOnUnchecked(object sender, RoutedEventArgs e) {
            dataGrid.Columns[1].Visibility = Visibility.Collapsed;
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
