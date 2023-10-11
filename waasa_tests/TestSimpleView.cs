using System.Text.Json;
using waasa.Models;
using waasa.Services;
using Serilog;


namespace waasa_tests {

    [TestClass]
    public class TestSimpleView {
        static GatheredDataSimpleView? getData() {
            var filename = "gathereddata-small.json";
            if (!File.Exists(filename)) {
                Log.Information("  Not found. No data.");
                return null;
            }
            string jsonString = File.ReadAllText(filename);
            var GatheredData = JsonSerializer.Deserialize<_GatheredData>(jsonString)!;
            var Registry = new GatheredDataSimpleView(GatheredData);
            return Registry;
        }

        [TestMethod]
        public void TestVirtRegHkcr() {
            var Registry = getData();
            Assert.IsNotNull(Registry);

            Assert.AreEqual(true, Registry.hasRootDefault(".386"));
            Assert.AreEqual("vxdfile", Registry.getRootDefault(".386"));
            Assert.AreEqual("system", Registry.getRootPerceivedType(".386"));
            Assert.AreEqual("{098f2470-bae0-11cd-b579-08002b30bfeb}", Registry.getRootPersistentHandler(".386"));
            Assert.AreEqual(0, Registry.countRootProgids(".386"));

            Assert.AreEqual("appx9rkaq77s0jzh1tyccadx9ghba15r6t3h", Registry.getRootProgid(".3fr"));

            Assert.AreEqual("VLC.3g2", Registry.getRootDefault(".3g2"));
            Assert.AreEqual(6, Registry.countRootProgids(".3g2"));
            Assert.AreEqual("video/3gpp2", Registry.getRootContentType(".3g2"));
            Assert.AreEqual("", Registry.GetExecutableForObjid(".3g2"));

            Assert.IsTrue(Registry.GetExecutableForObjid(".fluid").Contains("OneDrive\\23"));


            // isValidRootDefault()
            // isValidRootProgids()
            // hasValidRootProgidsToasts()
        }

        [TestMethod]
        public void TestVirtRegUser() {
            var Registry = getData();
            Assert.IsNotNull(Registry);
            Assert.AreEqual("AppX6eg8h5sxqq90pv53845wmnbewywdqq5h", Registry.getUserChoice(".3g2"));
            Assert.AreEqual(2, Registry.countUserOpenWithProgids(".3g2"));
            Assert.AreEqual("2", Registry.countUserOpenWithList(".3fr"));
            // getUserOpenWithProgids
            // getUserOpenWithList
            // isUserListValid
            // allUserProgidsValid
            // isValidUserProgids

        }
    }
}