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

namespace waasa
{
    public partial class WindowInfo : Window
    {
        public class InfoData
        {
            public string ExtensionRegInfo { get; set; }
            public string ExtensionObjInfo { get; set; }
            public string Shlwapi { get; set; }
        }

        
        public WindowInfo(_GatheredData gatheredData, _FileExtension fe)
        {
            InitializeComponent();

            InfoData infoData = new InfoData();
            infoData.ExtensionRegInfo = gatheredData.GetExtensionInfo(fe.Extension);
            //infoData.ExtensionObjInfo = gatheredData.GetObjidInfo(fe.Extension);
            infoData.Shlwapi = gatheredData.GetShlwapiInfo(fe.Extension);

            this.DataContext = infoData;
        }
    }
}
