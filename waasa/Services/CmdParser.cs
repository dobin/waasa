using Serilog;


namespace waasa.Services {
    using System;
    using System.Runtime.InteropServices;

    public class CmdParser {
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
            out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine) {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++) {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }
                return args;
            } finally {
                Marshal.FreeHGlobal(argv);
            }
        }


        public static (string, string) CommandLineToResult(string commandlLine) {
            if (commandlLine == "") {
                return ("", "");
            }

            if (commandlLine.StartsWith("?")) {
                return ("? 2", "");
            }

            var args = CommandLineToArgs(commandlLine);
            if (args.Length == 0) {
                return ("? 3", "");
            }

            string cmd = args[0];
            string argStr = "";
            for (var i = 1; i<args.Length; i++) {
                argStr += '\"' + args[i] + "\" ";
            }

            if (cmd == "C:\\Program") {
                // Wrong resolve, path is missing quotes...
                // Re-do it myself
                var idx = commandlLine.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
                if (idx > 0) {
                    // Add first quote
                    commandlLine = "\"" + commandlLine;
                    // Add second quote
                    commandlLine.Insert(idx, "\"");
                    // do it again and return result (this time it should be correct)
                    return CommandLineToResult(commandlLine);
                }
            }

            if (cmd.Contains("waaza.exe")) {
                // Wrong resolve...
                Log.Warning("Cmd resolved to myself: " + commandlLine);
            }

            return (cmd, argStr.Trim());
        }
    }
}
