using System.Text.Json;
using waasa.Models;
using waasa.Services;


namespace waasa_tests {
    [TestClass]
    public class TestStuff {
        [TestMethod]
        public void TestCmdParser() {
            string cmd;
            var res = ("", "");

            cmd = "command \"arg with spaces\" argWithoutSpaces \"quoted arg\"";
            res = CmdParser.CommandLineToResult(cmd);
            Assert.AreEqual(res.Item1, "command");

            cmd = "\"c:\\\\program files(x86)\\something\\bla.exe\" args";
            res = CmdParser.CommandLineToResult(cmd);
            Assert.AreEqual(res.Item1, "c:\\\\program files(x86)\\something\\bla.exe");

            // Noteworthy
            cmd = "c:\\\\program files(x86)\\something\\bla.exe args";
            res = CmdParser.CommandLineToResult(cmd);
            Assert.AreEqual(res.Item1, "c:\\\\program");

            cmd = "? ";
            res = CmdParser.CommandLineToResult(cmd);
            Assert.AreEqual(res.Item1, "?");

            cmd = "";
            res = CmdParser.CommandLineToResult(cmd);
            Assert.AreEqual(res.Item1, "?");
        }
    }
}