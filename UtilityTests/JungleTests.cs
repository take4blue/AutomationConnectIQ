using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutomationConnectIQ.Lib.Tests
{
    [TestClass()]
    public class JungleTests
    {
        [TestMethod()]
        public void CreateTest()
        {
            var project = Jungle.Create(@"TestData\Test4\monkey.jungle");

            Assert.IsNotNull(project);
            Assert.AreEqual(project.EntryName, "DigiFuseApp");
            Assert.AreEqual(project.Devices.Count, 3);
            Assert.AreEqual(project.Devices[0], "d2air");
            Assert.AreEqual(project.Devices[1], "d2bravo_titanium");
            Assert.AreEqual(project.Devices[2], "vivolife");
            Assert.AreEqual(project.DefaultProgramPath, @"TestData\Test4\bin\DigiFuseApp.prg");
            Assert.IsTrue(project.IsValidDevice("d2bravo_titanium"));
            Assert.IsFalse(project.IsValidDevice("d2air1"));
        }
    }
}