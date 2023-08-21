using System;
using System.Net;
using VLTLaserControllerNET.Services;

namespace VLTLaserControllerNET
{
    public class VLTLaserController : VLTSender
    {
        public bool IsAlive => VLTLaserFinder.PingDevice(this.IPAddress);
        public bool IsPlay { get; set; } = false;
        public bool IsAutoturn { get; private set; }
        public int AutoturnTimeout { get; private set; }


        public override void Disconnect()
        {
            byte[] args = new byte[1] { 0 };
            VLTMessage request = SendCommand("LINK", args, ReciveTimeout);

            if (request.Message.StartsWith("dis") == true)
            {
                this.IsAutoturn = request.Bytes[3] == 0x01;
                this.AutoturnTimeout = int.Parse(request.Bytes[4].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
            }
            base.Disconnect();
        }

        public VLTLaserController(IPAddress address, int number = 0)
        {
            this.IPAddress = address;
            this.SendPort = 5011 + number;
        }



        public void TurnPlay(bool On)
        {
            byte[] arg = new byte[1] 
            {
                Convert.ToByte(On)
            };
            SendCommand("PLAY", arg);
            this.IsPlay = On;
        }

        // Необходимо обеспечивать последовательное соединение проекторов и наче проекторы будут слать все на один порт

        public void Reset()
        {
            SendCommand("RESET", ReciveTimeout);
            while (this.IsAlive == false) { }
            base.Disconnect();
            base.Connect();
        }

        public void SetPort(ushort port)
        {
            ReceivePort = port;
            byte[] args = BitConverter.GetBytes(port);
            SendCommand("PORT", args, ReciveTimeout);
        }

        /// <summary>
        /// Set device color value
        /// </summary>
        /// <param name="colornum">1 - Red; 2 - Green; 3 - Blue</param>
        /// <param name="value">ushort max 655535</param>
        public void SetColor(byte colornum, ushort value)
        {
            string[] colorname = new string[3] 
            {
                "MRED",
                "MGRN",
                "MBLU"
            };
            byte[] args = BitConverter.GetBytes(value);
            SendCommand(colorname[colornum], args);
        }

        public void TurnAutoOff(bool On, byte minutes)
        {
            byte[] bytes = new byte[2] 
            {
                Convert.ToByte(On),
                minutes 
            };
            SendCommand("ATOF", bytes);
        }

        /// <summary>
        /// Save setting in permanent memmory on hardaware
        /// </summary>
        /// <param name="param">1 - Save ALL setting (need reset); 2 - ALL setting without ethernet (don't reset)</param>
        public void SaveSetting(byte param)
        {
            byte[] args = new byte[1] { param };
            SendCommand("SAVED", args);
        }

        /// <summary>
        /// Turn On/Off web server
        /// </summary>
        /// <param name="status">1 - On; 0 - Off</param>
        public void TurnWebServer(bool status)
        {
            byte[] args = new byte[1] 
            {
                Convert.ToByte(status)
            };
            SendCommand("WEBSR", args);
        }

        public void SetFlip(bool x, bool y)
        {
            byte[] argx = new byte[1] { Convert.ToByte(x) };
            SendCommand("FLIPX", argx);
            byte[] argy = new byte[1] { Convert.ToByte(y) };
            SendCommand("FLIPY", argy);
        }


        public bool WakeUpDevice()
        {
            VLTMessage request = SendCommand("QUERY", ReciveTimeout);
            return request.IsEmpty == false && request.Message.StartsWith("act");
        }

        public void SendScan(short delay)
        {
            byte[] bytes = BitConverter.GetBytes(delay);
            this.SendCommand("SCAN", bytes);
        }

        public void SendShift(byte shift)
        {
            SendCommand("SHIF", shift);
        }
    }


}
