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
            var input = File.ReadAllText("data.yaml");
            var deserializer = new DeserializerBuilder().Build();
            var yamlRoot = deserializer.Deserialize<YamlRoot>(input);

            foreach (var dataExtension in yamlRoot.Extensions) {
                Log.Information("DataParser: " + dataExtension.Extension);
                foreach(var tag in dataExtension.Tags) {
                    Log.Information("- " + tag);
                }
            }

            return yamlRoot.Extensions;
        }
    }
}
