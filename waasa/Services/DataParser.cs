using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using Serilog;


namespace waasa.Services
{
    public class DataExtension {
        public string Extension { get; set; } = "";
        public string Description { get; set; } = "";
        public string Exec{ get; set; } = "";
        public string Confidence { get; set; } = "";
        public bool Common { get; set; } = false;

        public string MitreInitialAccess { get; set; } = "";
        public string MitreExecution { get; set; } = "";
        public bool WindowsBuiltin { get; set; } = false;
        public string Notes { get; set; } = "";
        public bool Container { get; set; } = false;
        public bool BusinessCase { get; set; } = false;
        public bool Code { get; set; } = false;
        public List<String> Tags { get; set; } = new List<string>();
    }

    public class YamlRoot {
        public List<DataExtension> Extensions { get; set; } = new List<DataExtension>();
    }


    class DataParser
    {
        public static List<DataExtension> ReadYaml(string filepath) {
            if (!File.Exists(filepath)) {
                return new List<DataExtension>();
            }
            var input = File.ReadAllText(filepath);
            var deserializer = new DeserializerBuilder().Build();
            var yamlRoot = deserializer.Deserialize<YamlRoot>(input);

            return yamlRoot.Extensions;
        }
    }
}
