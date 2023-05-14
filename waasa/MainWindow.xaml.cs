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

namespace waasa
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(_GatheredData gatheredData, List<_FileExtension> fileExtensions)
        {
            InitializeComponent();
            dataGrid.ItemsSource = fileExtensions;
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
            _FileExtension person = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{person.Extension} {person.Result} clicked the button!");
        }

        private void ButtonBrowserDownload(object sender, RoutedEventArgs e)
        {
            _FileExtension person = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{person.Extension} {person.Result} clicked the button!");
        }

        private void ButtonDetails(object sender, RoutedEventArgs e)
        {
            _FileExtension person = (_FileExtension)(sender as Button).DataContext;
            Console.WriteLine($"ButtonExec{person.Extension} {person.Result} clicked the button!");
        }
    }
}
