using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using waasa.Models;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Diagnostics;
using Serilog;


namespace waasa.Services {
    class Io {

        static public List<_FileExtension> ReadResultJson(string filepath) {
            if (!File.Exists(filepath)) {
                return new List<_FileExtension>();
            }

            Log.Information("Reading JSON from: " + filepath);
            var jsonString = File.ReadAllText(filepath);
            var fileExtensions = JsonSerializer.Deserialize<List<_FileExtension>>(jsonString);
            return fileExtensions;
        }


        static public void ExecFile(string extension) {
            string filepath = System.Environment.GetEnvironmentVariable("TEMP") + "\\test" + extension;
            File.Create(filepath);
            ProcessStartInfo startInfo = new ProcessStartInfo("explorer.exe");
            startInfo.Arguments = filepath;
            Process.Start(startInfo);
        }


        static public void WriteResultJson(List<_FileExtension> fileExtensions, string filepath) {
            Log.Information("Writing JSON to: " + filepath + " with " + fileExtensions.Count);
            using (StreamWriter writer = new StreamWriter(filepath)) {
                string strJson = JsonSerializer.Serialize(fileExtensions);
                writer.WriteLine(strJson);
            }
        }


        static public _GatheredData GatherDataFromSystem() {
            var gather = new Gatherer();
            var gatheredData = gather.GatherAll();
            return gatheredData;
        }

        static public void WriteGatheredData(_GatheredData gatheredData, string filepath) {
            using (StreamWriter writer = new StreamWriter(filepath)) {
                string strJson = JsonSerializer.Serialize(gatheredData);
                writer.WriteLine(strJson);
            }
        }

        static public _GatheredData ReadGatheredData(string filepath) {
            var jsonString = File.ReadAllText(filepath);
            var gatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString);
            return gatheredData;
        }


        static public List<_FileExtension> DumpFromSystem() {
            var gather = new Gatherer();
            var gatheredData = gather.GatherAll();
            var SimpleRegistryView = new GatheredDataSimpleView(gatheredData);
            var validator = new Validator();
            var analyzer = new Analyzer();
            analyzer.Load(gatheredData, validator, SimpleRegistryView);
            List<_FileExtension> fileExtensions = analyzer.getResolvedFileExtensions();
            return fileExtensions;
        }

        static public List<_FileExtension> QuickDumpFromSystem() {
            var gatherer = new Gatherer();
            return gatherer.QuickGatherExtensions();
        }


        static public void WriteResultsToCsv(List<_FileExtension> fileExtensions, string filepath) {
            Log.Information("Writing CSV to: " + filepath + " with " + fileExtensions.Count);

            List<_CsvEntry> csvEntries = new List<_CsvEntry>();
            foreach (var fileExtension in fileExtensions) {
                csvEntries.Add(new _CsvEntry(fileExtension));
            }

            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(csvEntries);
        }


        static public void usageCreateResultsCsvDebug(string filepath, List<_FileExtension> fileExtensions, GatheredDataSimpleView registry) {
            var fileExtensionsDebug = registry.GetFileExtensionDebug(fileExtensions);

            Log.Information("Writing CSVDebug to: " + filepath + " with " + fileExtensionsDebug.Count);
            using var writer = new StreamWriter(filepath);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { };
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(fileExtensionsDebug);
        }


        static public void GenerateFiles(List<_FileExtension> fileExtensions) {
            foreach (var app in fileExtensions) {
                var output = "output";
                var filename = "test" + app.Extension;
                var directory = app.Assumption;

                if (!Directory.Exists(output)) {
                    Directory.CreateDirectory(output);
                }
                if (!Directory.Exists(output + "\\" + directory)) {
                    Directory.CreateDirectory(output + "\\" + directory);
                }
                File.Create(output + "\\" + directory + "\\" + filename);
            }
        }

        static public List<_FileExtension> ReadManual(string filepath) {
            List<_FileExtension> fileExtensions = new List<_FileExtension>();

            try {
                using (StreamReader sr = new StreamReader(filepath)) {
                    string? line;
                    while ((line = sr.ReadLine()) != null) {
                        if (line == "") {
                            continue;
                        }
                        var fe = new _FileExtension(line);
                        fileExtensions.Add(fe);
                    }
                }
            } catch (IOException ex) {
                Log.Information("The file could not be read:");
                Log.Information(ex.Message);
            }

            return fileExtensions;
        }
    }
}
