using System;
using System.Text;
using System.Runtime.InteropServices;


/// <summary>
/// Interface to query windows shlwapi.dll for file associations 
/// </summary>
public class Winapi {
    [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]

    static extern uint AssocQueryString(
        AssocF flags, AssocStr str, string pszAssoc, string pszExtra,
        [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);


    // https://learn.microsoft.com/en-us/windows/win32/shell/assocf_str
    [Flags]
    public enum AssocF {
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
    public enum AssocStr {
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

    public static string AssocQueryString(AssocStr assocStr, string doctype) {
        uint pcchOut = 0;  // size of output buffer

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
    public class WinapiEntry {
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

        public override string ToString() {
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

    public static WinapiEntry Query(string ext) {
        WinapiEntry assoc = new WinapiEntry();
        assoc.Command = Winapi.AssocQueryString(Winapi.AssocStr.Command, ext);
        assoc.Executable = Winapi.AssocQueryString(Winapi.AssocStr.Executable, ext);
        assoc.FriendlyAppName = Winapi.AssocQueryString(Winapi.AssocStr.FriendlyAppName, ext);
        assoc.FriendlyDocName = Winapi.AssocQueryString(Winapi.AssocStr.FriendlyDocName, ext);
        assoc.NoOpen = Winapi.AssocQueryString(Winapi.AssocStr.NoOpen, ext);
        assoc.ShellNewValue = Winapi.AssocQueryString(Winapi.AssocStr.ShellNewValue, ext);
        assoc.DDECommand = Winapi.AssocQueryString(Winapi.AssocStr.DDECommand, ext);
        assoc.DDEIfExec = Winapi.AssocQueryString(Winapi.AssocStr.DDEIfExec, ext);
        assoc.DDEApplication = Winapi.AssocQueryString(Winapi.AssocStr.DDEApplication, ext);
        assoc.ContentType = Winapi.AssocQueryString(Winapi.AssocStr.CONTENTTYPE, ext);
        assoc.SupportedUri = Winapi.AssocQueryString(Winapi.AssocStr.SUPPORTED_URI_PROTOCOLS, ext);
        assoc.Progid = Winapi.AssocQueryString(Winapi.AssocStr.PROGID, ext);
        assoc.AppId = Winapi.AssocQueryString(Winapi.AssocStr.APPID, ext);
        assoc.AppPublisher = Winapi.AssocQueryString(Winapi.AssocStr.APPPUBLISHER, ext);
        return assoc;
    }

}