using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


// Interface to query windows shlwapi.dll for file associations
public class Shlwapi
{
    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]

    static extern uint AssocQueryString(
        AssocF flags, AssocStr str, string pszAssoc, string pszExtra, 
        [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);


    // https://learn.microsoft.com/en-us/windows/win32/shell/assocf_str
    [Flags]
    public enum AssocF
    {
        None = 0x0,
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
        IgnoreBaseClass = 0x200,

        ASSOCF_INIT_IGNOREUNKNOWN = 0x00000400,
        ASSOCF_INIT_FIXED_PROGID = 0x00000800,
        ASSOCF_IS_PROTOCOL = 0x00001000,
        ASSOCF_INIT_FOR_FILE = 0x00002000
    }


    // https://learn.microsoft.com/en-us/windows/win32/api/shlwapi/ne-shlwapi-assocstr
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
        DDETopic,

        INFOTIP,
        AQUICKTIP,
        TILEINFO,
        CONTENTTYPE,
        DEFAULTICON,
        SHELLEXTENSION,
        DROPTARGET,
        DELEGATEEXECUTE,
        SUPPORTED_URI_PROTOCOLS,
        PROGID,
        APPID,
        APPPUBLISHER,
        APPICONREFERENCE,
    }

    public static string AssocQueryString(AssocStr assocStr, string doctype)
    {
        uint pcchOut = 0;   // size of output buffer

        // First call is to get the required size of output buffer
        AssocQueryString(AssocF.Verify, assocStr, doctype, null, null, ref pcchOut);
        //Console.WriteLine(String.Format("Len: {0}: {1}", assocStr, pcchOut));
        StringBuilder pszOut = new StringBuilder((int)pcchOut);

        if (pcchOut > 0) {
            // Second call gets the actual string
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        } else {
            return "";
        }
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

        public string Executable { get; set; }
        public string ContentType { get; set; }
        public string SupportedUri { get; set; }
        public string Progid { get; set; }
        public string AppId { get; set; }
        public string AppPublisher { get; set; }

        public override string ToString()
        {
            string r = "";
            r += String.Format("FriendlyAppName: {0}\n", FriendlyAppName);
            r += String.Format("Command: {0}\n", Command);
            r += String.Format("FriendlyDocName: {0}\n", FriendlyDocName);
            r += String.Format("Executable: {0}\n", Executable);
            r += "\n";
            r += String.Format("NoOpen: {0}\n", NoOpen);
            r += String.Format("ShellNewValue: {0}\n", ShellNewValue);
            r += String.Format("DDECommand: {0}\n", DDECommand);
            r += String.Format("DDEIfExec: {0}\n", DDEIfExec);
            r += String.Format("DDEApplication: {0}\n", DDEApplication);
            r += "\n";
            r += String.Format("ContentType: {0}\n", ContentType);
            r += String.Format("SupportedUri: {0}\n", SupportedUri);
            r += String.Format("Progid: {0}\n", Progid);
            r += String.Format("AppId: {0}\n", AppId);
            r += String.Format("AppPublisher: {0}\n", AppPublisher);

            return r;
        }
    }

    public static Assoc Query(string data)
    {
        Assoc assoc = new Assoc();

        assoc.Command = Shlwapi.AssocQueryString(Shlwapi.AssocStr.Command, data);
        assoc.Executable = Shlwapi.AssocQueryString(Shlwapi.AssocStr.Executable, data);
        assoc.FriendlyAppName = Shlwapi.AssocQueryString(Shlwapi.AssocStr.FriendlyAppName, data);
        assoc.FriendlyDocName = Shlwapi.AssocQueryString(Shlwapi.AssocStr.FriendlyDocName, data);
        assoc.NoOpen = Shlwapi.AssocQueryString(Shlwapi.AssocStr.NoOpen, data);
        assoc.ShellNewValue = Shlwapi.AssocQueryString(Shlwapi.AssocStr.ShellNewValue, data);
        assoc.DDECommand = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDECommand, data);
        assoc.DDEIfExec = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDEIfExec, data);
        assoc.DDEApplication = Shlwapi.AssocQueryString(Shlwapi.AssocStr.DDEApplication, data);
        assoc.ContentType = Shlwapi.AssocQueryString(Shlwapi.AssocStr.CONTENTTYPE, data);
        assoc.SupportedUri = Shlwapi.AssocQueryString(Shlwapi.AssocStr.SUPPORTED_URI_PROTOCOLS, data);
        assoc.Progid = Shlwapi.AssocQueryString(Shlwapi.AssocStr.PROGID, data);
        assoc.AppId = Shlwapi.AssocQueryString(Shlwapi.AssocStr.APPID, data);
        assoc.AppPublisher = Shlwapi.AssocQueryString(Shlwapi.AssocStr.APPPUBLISHER, data);

        return assoc;
    }
}