using System.Windows;


namespace waasa
{
    /// <summary>
    /// Interaction logic for WindowSearch
    /// </summary>
    public partial class WindowSearch : Window
    {
        public string UserInput { get; private set; } = "";

        public WindowSearch()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            UserInput = InputTextBox.Text;
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }
    }
}
