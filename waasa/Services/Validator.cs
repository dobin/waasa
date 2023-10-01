using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using waasa.Models;


namespace waasa.Services {
    // Own opens.txt with fileextension:result entries, and allows queries
    public class Validator {
        private Dictionary<string, string> tests { get; } = new Dictionary<string, string>();


        public Validator() {
        }


        // Parse opens.txt
        public void LoadFromFile(string opensFilename) {
            if (!File.Exists(opensFilename)) {
                return;
            }

            Console.WriteLine("Validator: " + opensFilename);
            using (var fileStream = File.OpenRead(opensFilename))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 512)) {
                string line;
                while ((line = streamReader.ReadLine()) != null) {
                    // Process line
                    var s = line.Split(':');

                    if (s.Count() != 2) {
                        Console.WriteLine("Error: Invalid Line: " + line);
                        continue;
                    }
                    if (s[0] == "" || s[1] == "") {
                        Console.WriteLine("Error: Invalid Line: " + line);
                        continue;
                    }

                    if (!tests.ContainsKey(s[0])) {
                        tests.Add(s[0], s[1]);
                    } else {
                        //Console.WriteLine("Error: Double Entry on line: " + line);
                    }
                }
            }
        }


        public string GetEffectiveResultFor(string extension) {
            if (tests.ContainsKey(extension)) {
                return tests[extension];
            } else {
                return "";
            }
        }


        public void PrintStats(List<_FileExtension> fileExtensions) {
            Console.WriteLine("Missmatches:");
            var n = 0;
            var eExec = 0;
            var eRecommended = 0;
            var eOpenwith = 0;
            var amount = 0;
            foreach (var app in fileExtensions) {
                amount++;
                if (app.Assumption != null && tests.ContainsKey(app.Extension)) {
                    var effective = app.Assumption;
                    var identified = app.Result;

                    if (effective.StartsWith(identified)) {
                        //Console.WriteLine("  OK: " + tests[app.AppData.Name].Result);
                    } else {
                        Console.WriteLine("  Missmatch: " + app.Extension + ": " + identified + " <-> " + effective);
                        n += 1;

                        switch (identified) {
                            case "exec":
                                eExec += 1;
                                break;
                            case "recommended":
                                eRecommended += 1;
                                break;
                            case "openwith":
                                eOpenwith += 1;
                                break;
                        }

                    }
                }
            }

            Console.WriteLine("Validator Stats:");
            Console.WriteLine("  AMount: " + amount);
            Console.WriteLine("  Erros: " + n);
            Console.WriteLine("  Erros Exec:        " + eExec);
            Console.WriteLine("  Erros Recommended: " + eRecommended);
            Console.WriteLine("  Erros openwith:    " + eOpenwith);
        }
    }
}
