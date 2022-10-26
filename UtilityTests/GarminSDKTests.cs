using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomationConnectIQ.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AutomationConnectIQ.Lib.Tests
{
    public class EnvSupport : IEnvironment
    {
        public string env_ = "";

        public string AppBase { get => env_; }
    }

    [TestClass()]
    public class GarminSDKTests
    {
        [TestMethod()]
        public void GarminTest1()
        {
            var env = new EnvSupport()
            {
                env_ = @"TestData\Test1"
            };
            var garmin = new GarminSDK(env);

            Assert.AreEqual(garmin.SdkFolder, @"TestData\Test1\Sdk\Sdk2");
            Assert.AreEqual(garmin.Version, "3.2.3");
            Assert.AreEqual(garmin.CompareVersion("3.2.3"), 0);
            Assert.AreEqual(garmin.CompareVersion("3.2.0"), -1);
            Assert.AreEqual(garmin.CompareVersion("3.2.4"), 1);
            Assert.AreEqual(garmin.CompareVersion("3.1.3"), -1);
            Assert.AreEqual(garmin.CompareVersion("3.3.3"), 1);
            Assert.AreEqual(garmin.CompareVersion("2.2.3"), -1);
            Assert.AreEqual(garmin.CompareVersion("4.2.3"), 1);
            Assert.AreEqual(garmin.CompareVersion("3.1"), -1);
            Assert.AreEqual(garmin.CompareVersion("3.3"), 1);
            Assert.AreEqual(garmin.CompareVersion("2"), -1);
            Assert.AreEqual(garmin.CompareVersion("4"), 1);
        }

        [TestMethod()]
        public void GarminTest2()
        {
            var env = new EnvSupport()
            {
                env_ = @"TestData\Test2"
            };
            var garmin = new GarminSDK(env);

            Assert.AreEqual(garmin.SdkFolder, @"TestData\Test2\Sdk\Sdk1");
            Assert.AreEqual(garmin.Version, "3.1.1");
        }

        [TestMethod()]
        public void GarminTest4()
        {
            var env = new EnvSupport()
            {
                env_ = @"TestData\Test3"
            };
            try {
                var garmin = new GarminSDK(env);
            }
            catch (DirectoryNotFoundException ex) {
                Assert.AreEqual(ex.Message, @"TestData\Test3\Sdk\Sdk1");
                return;
            }
            Assert.Fail();
        }

        [TestMethod()]
        public void GarminTest5()
        {
            var env = new EnvSupport()
            {
                env_ = @"TestData\Hoge"
            };
            try {
                var garmin = new GarminSDK(env);
            }
            catch (DirectoryNotFoundException ex) {
                Assert.AreEqual(ex.Message, @"TestData\Hoge");
                return;
            }
            Assert.Fail();
        }

    }
}