using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
            DataContext = new TextboxViewModel();
        }

        private async void ButtonDownload(object sender, RoutedEventArgs e) {
            var viewModel = (TextboxViewModel)DataContext;

            viewModel.OutputTextBox = "Downloading: " + FileExtension.Extension + " ...";
            HttpAnswerInfo answer = await Requestor.Get("test" + FileExtension.Extension);
            viewModel.OutputTextBox = answer.StatusCode + " " + answer.Filename + " " + answer.Content;
        }


        public class TextboxViewModel : INotifyPropertyChanged {
            private string _text = "";
            public string OutputTextBox {
                get { return _text; }
                set {
                    if (_text != value) {
                        _text = value;
                        OnPropertyChanged(nameof(OutputTextBox));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected void OnPropertyChanged(string propertyName) {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
