using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
//using YamlDotNet.Serialization;
using System.Xml;
using Microsoft.Win32;


namespace waasa
{
    [Serializable]
    public class cTested
    {
        public string Result { get; set; }
    }


    class Validator
    {
        private Dictionary<string, cTested> tests { get; } = new Dictionary<string, cTested>();


        public Validator()
        {

        }


        public void Load(string opensFilename)
        {
            Console.WriteLine("Validator: " + opensFilename);
            // Parse opens.txt
            using (var fileStream = File.OpenRead(opensFilename))
            using (var streamReader = new StreamReader(fileStream, System.Text.Encoding.UTF8, true, 512)) {
                String line;
                while ((line = streamReader.ReadLine()) != null) {
                    // Process line
                    var s = line.Split(':');
                    //Console.WriteLine($"b: {s[0]}");

                    var tested = new cTested();
                    tested.Result = s[1];
                    if (! tests.ContainsKey(s[0])) {
                        tests.Add(s[0], tested);
                    } else {
                        //Console.WriteLine("Double: " + s[0]);
                    }
                }
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


        public void Validate(List<_FileExtension> fileExtensions)
        {
            Console.WriteLine("Validating...");
            foreach (var app in fileExtensions) {
                if (app.Assumption != null && tests.ContainsKey(app.Extension)) {
                    var effective = app.Assumption;
                    var identified = tests[app.Extension].Result;
                    app.Result = identified;
                }
            }
        }

        public void ValidateDebug(List<_FileExtensionDebug> fileExtensions)
        {
            Console.WriteLine("Validating...");
            foreach (var app in fileExtensions) {

                if (app.Assumption != null && tests.ContainsKey(app.Extension)) {
                    app.Result = tests[app.Extension].Result;
                }
            }
        }
    }
}
