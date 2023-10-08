using System.Windows;
using waasa.Services;
using waasa.Models;


namespace waasa
{
    /// <summary>
    /// Detailed Information about a file extension
    /// </summary>
    public partial class WindowInfo : Window
    {
        public class InfoData
        {
            public string ExtensionRegInfo { get; set; } = "";
            public string ExtensionObjInfo { get; set; } = "";
            public string Shlwapi { get; set; } = "";
        }

        
        public WindowInfo(_FileExtension fe)
        {
            InitializeComponent();

            InfoData infoData = new InfoData();
            //infoData.ExtensionRegInfo = gatheredData.GetExtensionInfo(fe.Extension);
            //infoData.ExtensionObjInfo = gatheredData.GetObjidInfo(fe.Extension);
            //infoData.Shlwapi = gatheredData.GetShlwapiInfo(fe.Extension);

            this.DataContext = infoData;
        }
    }
}
