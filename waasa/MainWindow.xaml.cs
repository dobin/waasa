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
using System.ComponentModel;

namespace waasa
{
    public partial class MainWindow : Window
    {
        // UI mostly done with ChatGPT
        private _GatheredData GatheredData;
        private string DumpFilepath;
        private string OpensFilepath;
        private List<_FileExtension> FileExtensions;

        ICollectionView collectionView;
        private string searchFilter = "";


        public MainWindow(string dumpFilepath, string opensFilepath)
        {
            InitializeComponent();

            DataContext = new MyViewModel(this);

            DumpFilepath = dumpFilepath;
            OpensFilepath = opensFilepath;

            load();
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

            collectionView = CollectionViewSource.GetDefaultView(FileExtensions);
            collectionView.Filter = obj =>
            {
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

            // UI
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

        private void Menu_Filter(object sender, RoutedEventArgs e)
        {
            SetSearchFilter();
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

        public void SetSearchFilter()
        {
            WindowSearch secondWindow = new WindowSearch();
            if (secondWindow.ShowDialog() == true) {
                searchFilter = secondWindow.UserInput;
                collectionView.Refresh();
            }
        }
    }


    public class MyViewModel : INotifyPropertyChanged
    {
        MainWindow mywindow;

        public MyViewModel(MainWindow mainwindow) : base()
        {

            mywindow = mainwindow;
        }

        private ICommand searchCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand SearchCommand
        {
            get
            {
                if (searchCommand == null) {
                    searchCommand = new RelayCommand(
                        param => this.Search(),
                        param => this.CanSearch()
                    );
                }
                return searchCommand;
            }
        }

        private void Search()
        {
            mywindow.SetSearchFilter();
        }

        private bool CanSearch()
        {
            return true;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
