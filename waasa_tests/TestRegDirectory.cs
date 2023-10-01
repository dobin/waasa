

namespace waasa_tests {
    [TestClass]
    public class TestRegDirectory {
        [TestMethod]
        public void TestRegModelAccess() {
            var d1 = new waasa.Models._RegDirectory("level1");
            var d21 = new waasa.Models._RegDirectory("level2.1");
            var d22 = new waasa.Models._RegDirectory("level2.2");
            var d3 = new waasa.Models._RegDirectory("level3");

            d1.AddDir("level2.1", d21);
            d1.AddDir("level2.2", d22);
            d21.AddDir("level3", d3);
            Assert.AreEqual(d1.GetDir("level2.1").Name, "level2.1");
            Assert.AreEqual(d1.GetDir("level2.1\\level3").Name, "level3");
            Assert.AreEqual(d1.GetDir("level2.1\\asdf"), null);

            d1.AddKey("Test1", "test1");
            d3.AddKey("Test3", "test3");
            Assert.AreEqual(d1.GetKey("xxx"), "");
            Assert.AreEqual(d1.GetKey("Test1"), "test1");
            Assert.AreEqual(d1.GetKey("level2.1\\level3\\Test3"), "test3");
        }

        [TestMethod]
        public void TestRegModelDefault() {
            var d1 = new waasa.Models._RegDirectory("level1");
            var d2 = new waasa.Models._RegDirectory("level2");

            d1.AddDir("level2", d2);
            d1.AddKey("", "default1");
            d2.AddKey("", "default2");

            Assert.AreEqual(d1.GetKey(""), "default1");
            Assert.AreEqual(d1.GetKey("(Default)"), "default1");
            Assert.AreEqual(d1.GetKey("level2\\"), "default2");
            Assert.AreEqual(d1.GetKey("level2\\(Default)"), "default2");
        }
    }
}