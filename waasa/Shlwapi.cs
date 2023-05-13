using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


public class Shlwapi
{
    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

    [Flags]
    public enum AssocF
    {
        Init_NoRemapCLSID = 0x1,
        Init_ByExeName = 0x2,
        Open_ByExeName = 0x2,
        Init_DefaultToStar = 0x4,
        Init_DefaultToFolder = 0x8,
        NoUserSettings = 0x10,
        NoTruncate = 0x20,
        Verify = 0x40,
        RemapRunDll = 0x80,
        NoFixUps = 0x100,
        IgnoreBaseClass = 0x200
    }

    public enum AssocStr
    {
        Command = 1,
        Executable,
        FriendlyDocName,
        FriendlyAppName,
        NoOpen,
        ShellNewValue,
        DDECommand,
        DDEIfExec,
        DDEApplication,
        DDETopic
    }

    public static string AssocQueryString(AssocStr assocStr, string doctype)
    {
        uint pcchOut = 0;   // size of output buffer

        // First call is to get the required size of output buffer
        AssocQueryString(AssocF.Verify, assocStr, doctype, null, null, ref pcchOut);

        StringBuilder pszOut = new StringBuilder((int)pcchOut);

        // Second call gets the actual string
        AssocQueryString(AssocF.Verify, assocStr, doctype, null, pszOut, ref pcchOut);

        return pszOut.ToString();
    }

    [Serializable]
    public class Assoc
    {
        public string FriendlyAppName { get; set; }
        public string Command { get; set; }
        public string FriendlyDocName { get; set; }
        public string NoOpen { get; set; }
        public string ShellNewValue { get; set; }
        public string DDECommand { get; set; }
        public string DDEIfExec { get; set; }
        public string DDEApplication { get; set; }

        public override string ToString()
        {
            string r = "";
            r += String.Format("FriendlyAppName: {0}\n", FriendlyAppName);
            r += String.Format("Command: {0}\n", Command);
            r += String.Format("FriendlyDocName: {0}\n", FriendlyDocName);
            r += String.Format("NoOpen: {0}\n", NoOpen);
            r += String.Format("ShellNewValue: {0}\n", ShellNewValue);
            r += String.Format("DDECommand: {0}\n", DDECommand);
            r += String.Format("DDEIfExec: {0}\n", DDEIfExec);
            r += String.Format("DDEApplication: {0}", DDEApplication);
            return r;
        }
    }

    public static Assoc Query(string data)
    {
        Assoc assoc = new Assoc();

        assoc.FriendlyAppName = Shlwapi.AssocQueryString(Shlwapi.AssocStr.FriendlyAppName, data);
        assoc.Command = Shlwapi.AssocQueryString(Shlwapi.AssocStr.Command, data);
        assoc.FriendlyDocName = Shlwapi.AssocQueryString(Shlwapi.AssocStr.FriendlyDocName, data);
        assoc.NoOpen = Shlwapi.AssocQueryString(Shlwapi.AssocStr.NoOpen, data);
        assoc.ShellNewValue = Shlwapi.AssocQueryString(Shlwapi.AssocStr.ShellNewValue, data);
        assoc.DDECommand = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDECommand, data);
        assoc.DDEIfExec = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDEIfExec, data);
        assoc.DDEApplication = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDEApplication, data);

        if (false) {
            Console.WriteLine("FriendlyAppName: " + assoc.FriendlyAppName);
            Console.WriteLine("Command: " + assoc.Command);
            Console.WriteLine("FriendlyDocName: " + assoc.FriendlyDocName);
            Console.WriteLine("NoOpen: " + assoc.NoOpen);
            Console.WriteLine("ShellNewValue: " + assoc.ShellNewValue);
            Console.WriteLine("DDECommand: " + assoc.DDECommand);
            Console.WriteLine("DDEIfExec: " + assoc.DDEIfExec);
            Console.WriteLine("DDEApplication: " + assoc.DDEApplication);
        }

        return assoc;
    }
}