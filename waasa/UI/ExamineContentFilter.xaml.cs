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

namespace waasa.UI
{
    /// <summary>
    /// Interaction logic for ExamineContentFilter.xaml
    /// </summary>
    public partial class ExamineContentFilter : Window
    {
        public ExamineContentFilter()
        {
            InitializeComponent();
            DataContext = new DownloadWindowDataModel2();
        }

        private async void ButtonTestContentFilter(object sender, RoutedEventArgs e) {
            PrintToUi("Checking...");
            string s = await ContentFilter.CheckContentFilter();
            //dataGrid.Items.Refresh();
            PrintToUi(s);
        }

        public class DownloadWindowDataModel2 : INotifyPropertyChanged {
            public string ServerTextBox { get; set; } = Properties.Settings.Default.WAASA_SERVER;
            public string OutputTextBox { get; set; } = "";

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
            var viewModel = (DownloadWindowDataModel2)DataContext;
            viewModel.LogTextBox += s + "\n";
        }

        private void ServerTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // Store new server value in config to persist it
            TextBox? textBox = sender as TextBox;
            if (textBox == null) {
                return;
            }
            string newServer = textBox.Text;
            Properties.Settings.Default.WAASA_SERVER = newServer;
            Properties.Settings.Default.Save();
        }
    }
}
