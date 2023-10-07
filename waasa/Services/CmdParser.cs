﻿using Serilog;


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
            if (commandlLine == "" || commandlLine.StartsWith("?")) {
                return ("?", "");
            }

            var args = CommandLineToArgs(commandlLine);
            if (args.Length == 0) {
                
            }

            string cmd = args[0];
            string argStr = "";
            for (var i = 1; i<args.Length; i++) {
                argStr += '\"' + args[i] + "\" ";
            }

            if (cmd.Contains("waaza.exe")) {
                // Wrong resolve...
                Log.Warning("Cmd resolved to myself: " + commandlLine);
            }

            return (cmd, argStr.Trim());
        }
    }
}