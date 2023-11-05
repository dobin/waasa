﻿using System.Windows;
using waasa.Services;
using waasa.Models;
using YamlDotNet.Serialization;


namespace waasa
{
    /// <summary>
    /// Detailed Information about a file extension. Requires GatheredData
    /// </summary>
    public partial class WindowInfo : Window
    {
        public class InfoData
        {
            public string ExtensionRegInfo { get; set; } = "";
            public string ExtensionObjInfo { get; set; } = "";
            public string Shlwapi { get; set; } = "";
            public string BadfileInfo { get; set; } = "";
        }

        
        public WindowInfo(_GatheredData gatheredData, _FileExtension fe)
        {
            InitializeComponent();

            InfoData infoData = new InfoData();
            infoData.ExtensionRegInfo = gatheredData.GetExtensionInfo(fe.Extension);
            infoData.ExtensionObjInfo = gatheredData.GetObjidInfo(fe.Extension);
            infoData.Shlwapi = gatheredData.GetShlwapiInfo(fe.Extension);

            if (fe.InfoExtension.Extension != "") {
                var serializer = new SerializerBuilder().Build();
                string yamlString = serializer.Serialize(fe.InfoExtension);
                infoData.BadfileInfo = yamlString;
            } else {
                infoData.BadfileInfo = "No data";
            }

            this.DataContext = infoData;
        }
    }
}
