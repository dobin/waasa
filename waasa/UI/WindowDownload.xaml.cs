using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Serilog;

using waasa.Models;
using waasa.Services;


namespace waasa.UI {
    /// <summary>
    /// Interaction logic for WindowDownload.xaml
    /// </summary>
    public partial class WindowDownload : Window {
        private _FileExtension FileExtension;

        public WindowDownload(_FileExtension fe) {
            InitializeComponent();
            FileExtension = fe;

            DataContext = new DownloadWindowDataModel();
            dataGrid.ItemsSource = FileExtension.TestResults;
            var viewModel = (DownloadWindowDataModel)DataContext;
            viewModel.FileExtensionLabel = fe.Extension;
        }

        private async void ButtonDownload(object sender, RoutedEventArgs e) {
            PrintToUi("Downloading...");
            BtnDownload.IsEnabled = false;
            await ContentFilter.analyzeExtension(FileExtension);
            dataGrid.Items.Refresh();
            PrintToUi("Success");
            BtnDownload.IsEnabled = true;
        }

        private void PrintToUi(string s) {
            var viewModel = (DownloadWindowDataModel)DataContext;
            viewModel.LogTextBox = s;
        }


        public class DownloadWindowDataModel : INotifyPropertyChanged {
            public string FileExtensionLabel { get; set; } = "";
            public string ServerTextBox { get; set; } = Properties.Settings.Default.WAASA_SERVER;

            /* Log Text Box update */
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
