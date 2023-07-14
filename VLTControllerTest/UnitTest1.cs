using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
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

        [TestMethod]
        public void TestSendFrame()
        {
            Thread.Sleep(100);
            VLTLaserController laserController = new VLTLaserController(_address, 5011);
            laserController.Connect();
            string path = "C:\\Users\\Urbaraban\\source\\repos\\VLTLaserController.net\\VLTControllerTest\\test_file\\v0_3d_full.ild";
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                if (reader.ReadBytes((int)reader.BaseStream.Length) is byte[] b)
                {
                    laserController.TurnPlay(true);
                    Assert.IsTrue(laserController.SendFrame(b));
                }
            }
            Thread.Sleep(5000);
            laserController.Disconnect();
        }
    }
}
