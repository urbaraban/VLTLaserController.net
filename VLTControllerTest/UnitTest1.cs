using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using VLTLaserControllerNET;

namespace VLTControllerTest
{
    [TestClass]
    public class UnitTest1
    {
        private IPAddress _address = new IPAddress(new byte[] { 192, 168, 33, 201 });

        [TestMethod]
        public void TestAlive()
        {
            VLTLaserController laserController = new VLTLaserController(_address, 5011);
            Assert.IsTrue(laserController.IsAlive);
        }

        [TestMethod]
        public void TestConnect()
        {
            Thread.Sleep(100);
            VLTLaserController laserController = new VLTLaserController(_address, 5011);
            laserController.Connect();
            Assert.IsTrue(laserController.IsConnected == true);
            laserController.Disconnect();
        }


        [TestMethod]
        public void TestDisconnect()
        {
            Thread.Sleep(100);
            VLTLaserController laserController = new VLTLaserController(_address, 5011);
            laserController.Connect();
            laserController.Disconnect();
            Assert.IsTrue(laserController.IsConnected == false);
        }

        [TestMethod]
        public void TestReset()
        {
            Thread.Sleep(100);
            VLTLaserController laserController = new VLTLaserController(_address, 5011);
            laserController.Connect();
            laserController.Reset();
            Assert.IsTrue(laserController.IsConnected == true);
            laserController.Disconnect();
        }
    }
}
