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
using System.Windows.Shapes;
using static waasa.UI.WindowDownload;
using waasa.Services;
using System.ComponentModel;
using waasa.Models;

namespace waasa.UI
{
    /// <summary>
    /// Interaction logic for ExamineContentFilter.xaml
    /// </summary>
    public partial class ExamineContentFilter : Window {

        public ExamineContentFilter() {
            InitializeComponent();
            DataContext = new ExamineContentFilterDataModel();
            dataGrid1.ItemsSource = new List<TestResult> {
                new TestResult(),
                new TestResult(),
                new TestResult()
            };
            dataGrid2.ItemsSource = new List<TestResult> {
                new TestResult(),
                new TestResult(),
                new TestResult()
            };
        }


        private async void ButtonTestContentFilter(object sender, RoutedEventArgs e) {
            PrintToUi("Checking...");
            BtnDownload.IsEnabled = false;
            _FileExtension fe;
            var viewModel = (ExamineContentFilterDataModel)DataContext;

            fe = new _FileExtension(viewModel.WhitelistedFile.ToLower());
            await ContentFilter.analyzeExtension(fe);
            dataGrid1.ItemsSource = fe.TestResults;
            dataGrid1.Items.Refresh();

            fe = new _FileExtension(viewModel.BlacklistedFile.ToLower());
            await ContentFilter.analyzeExtension(fe);
            dataGrid2.ItemsSource = fe.TestResults;
            dataGrid2.Items.Refresh();
            
            PrintToUi("done");
            BtnDownload.IsEnabled = true;
        }


        public class ExamineContentFilterDataModel : INotifyPropertyChanged {
            public string ServerTextBox { get; set; } = Properties.Settings.Default.WAASA_SERVER;
            public string OutputTextBox { get; set; } = "";
            public string WhitelistedFile { get; set; } = Properties.Settings.Default.WHITELISTED_EXTENSION;
            public string BlacklistedFile { get; set; } = Properties.Settings.Default.BLACKLISTED_EXTENSION;

            /* handle Log Text Box user input */
            private string _log = "";
            public string LogTextBox {
                get { return _log; }
                set {
                    if (_log != value) {
                        _log = value;
                        OnPropertyChanged(nameof(LogTextBox));
                    }
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void PrintToUi(string s) {
            var viewModel = (ExamineContentFilterDataModel)DataContext;
            viewModel.LogTextBox += s + "\n";
        }


        private void ServerTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) {
                return;
            }
            string newServer = textBox.Text;
            Properties.Settings.Default.WAASA_SERVER = newServer;
            Properties.Settings.Default.Save();
        }


        private void WhitelistedFileTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) {
                return;
            }
            string ext = textBox.Text;
            Properties.Settings.Default.WHITELISTED_EXTENSION = ext;
            Properties.Settings.Default.Save();
        }


        private void BlacklistedFileTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox? textBox = sender as TextBox;
            if (textBox == null) {
                return;
            }
            string ext = textBox.Text;
            Properties.Settings.Default.BLACKLISTED_EXTENSION = ext;
            Properties.Settings.Default.Save();
        }
    }
}
