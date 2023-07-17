using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VLTLaserControllerNET.Services;

namespace VLTLaserControllerNET
{
    public class VLTLaserController : IDisposable
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public bool IsAlive => VLTLaserFinder.PingDevice(this.IPAddress);
        public bool IsConnected => WakeUpDevice();
        public bool IsPlay { get; set; } = false;
        public bool IsAutoturn { get; private set; }
        public int AutoturnTimeout { get; private set; }

        public VLTLaserINFO VLTLaserInfo { get; private set; }

        public IPAddress IPAddress { get; set; } = new IPAddress(new byte[] { 192, 168, 1, 1 });
        public int SendPort { get; set; }
        public int ReceivePort { get; private set; }
        public int ReciveTimeout { get; set; } = 500;

        private UdpClient UdpSend { get; set; } = new UdpClient();

        private IPEndPoint SendEndPoint { get; set; }

        private UdpClient UdpReciver { get; set; }
        private IPEndPoint RecivedEndPoint { get; set; }

        private Task SendTask { get; set; }

        public VLTLaserController(IPAddress address, int port = 5011)
        {
            this.IPAddress = address;
            this.SendPort = port;
        }

        public async void SendFrame(byte[] frame, ushort scanrate, byte shift = 0)
        {
            int packetLength = 1458;
            int sessionLength = 5;
            int sessionsize = packetLength * sessionLength;

            var args = BitConverter.GetBytes(scanrate);
            SendCommand("SCAN", args);

            var shiftarg = new byte[1] { shift };
            SendCommand("SHIF", shiftarg);

            int frameSessionCount = (int)Math.Ceiling((double)frame.Length / (packetLength * sessionLength));
            for (int i = 0; i < frameSessionCount; i += 1)
            {
                for (int j = 0; j < Math.Min(sessionsize, frame.Length - sessionsize * i); j += packetLength)
                {
                    int skip = i * sessionsize + j;
                    byte[] packet = frame.Skip(skip).Take(packetLength).ToArray();
                    SendBytes(packet);
                }
                if (WaitBytes(ReciveTimeout).Message.StartsWith("act") == false)
                {
                    return;
                }
            }
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
        public void Connect()
        {
            if (this.UdpReciver != null)
            {
                this.Disconnect();
            }

            SendEndPoint = new IPEndPoint(IPAddress, SendPort);

            this.RecivedEndPoint = new IPEndPoint(IPAddress.Any, SendPort);
            this.UdpReciver = new UdpClient(RecivedEndPoint);

            byte[] args = new byte[1] { 1 };
            VLTMessage request = SendCommand("LINK", args, ReciveTimeout);

            if (request.IsEmpty == false && 
                request.IPEndPoint.Address.GetHashCode() == this.IPAddress.GetHashCode())
            {
                string message = request.Message;
                if (message.ToLower().StartsWith("vlt") == true)
                {
                    this.UdpReciver.Close();
                    UdpReciver.Dispose();
                    this.ReceivePort = (request.Bytes[3] << 8) | request.Bytes[4];
                    this.RecivedEndPoint = new IPEndPoint(IPAddress.Any, ReceivePort);
                    this.UdpReciver = new UdpClient(RecivedEndPoint);
                    UpdateInfo();
                    Connected?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                this.UdpReciver.Close();
                this.UdpReciver.Dispose();
                this.UdpReciver = null;
            }
        }

        public void UpdateInfo()
        {
            VLTMessage request = SendCommand("INFO", ReciveTimeout);
            this.VLTLaserInfo = new VLTLaserINFO(request.Message);
        }

        public void Disconnect()
        {
            byte[] args = new byte[1] { 0 };
            VLTMessage request = SendCommand("LINK", args, ReciveTimeout);

            if (request.Message.StartsWith("dis") == true)
            {
                this.IsAutoturn = request.Bytes[3] == 0x01;
                this.AutoturnTimeout = int.Parse(request.Bytes[4].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
            }
            if (this.UdpReciver != null)
            {
                this.UdpReciver.Close();
                this.UdpReciver.Dispose();
                this.UdpReciver = null;
            }
        }

        public void Reset()
        {
            SendCommand("RESET", ReciveTimeout);
            while (this.IsAlive == false) { }
            this.UdpReciver.Close();
            this.UdpReciver.Dispose();
            this.UdpReciver = null;
            this.Connect();
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

        public void Dispose()
        {
            this.Disconnect();
        }

        private VLTMessage SendCommand(string command, byte[] args, int timeoutrequest = 0)
        {
            if (command.Length + args.Length < 7)
            {
                byte[] bytes = ByteGetter.GetStringBytes(command, 6);
                for (int i = 0; i < args.Length; i += 1)
                {
                    bytes[command.Length + i] = args[i];
                    return SendCommand(bytes, timeoutrequest);
                }
            }
            return new VLTMessage();
        }

        private VLTMessage SendCommand(string command, int timeoutrequest = 0)
        {
            if (string.IsNullOrEmpty(command) == false && command.Length < 7)
            {
                byte[] bytes = ByteGetter.GetStringBytes(command, 6);
                return SendCommand(bytes, timeoutrequest);
            }
            return new VLTMessage();
        }

        private VLTMessage SendCommand(byte[] data, int timeoutrequest = 0)
        {
            if (data.Length < 7)
            {
                SendBytes(data);
                if (timeoutrequest > 0)
                {
                    return WaitBytes(timeoutrequest);
                }
            }
            return new VLTMessage();
        }

        private void SendBytes(byte[] bytes)
        {
            if (bytes.Length > 0 && SendEndPoint != null)
            {
                UdpSend.Send(bytes, bytes.Length, SendEndPoint);
            }
        }

        private VLTMessage WaitBytes(int timeout)
        {
            if (timeout > 0 && UdpReciver != null)
            {
                try
                {
                    IPEndPoint recive_end_point = this.RecivedEndPoint;
                    UdpReciver.Client.ReceiveTimeout = timeout;
                    byte[] request = UdpReciver.Receive(ref recive_end_point);
                    return new VLTMessage(request, recive_end_point);
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            return new VLTMessage();
        }

        private bool WakeUpDevice()
        {
            VLTMessage request = SendCommand("QUERY", ReciveTimeout);
            return request.IsEmpty == false && request.Message.StartsWith("act");
        }
    }

    internal struct VLTMessage
    {
        public IPEndPoint IPEndPoint { get; }
        public bool IsEmpty => Bytes == null || Bytes.Length == 0;
        public byte[] Bytes { get; set; } = new byte[0];
        public string Message
        {
            get
            {
                if (IsEmpty == true)
                {
                    return string.Empty;
                }
                return Encoding.ASCII.GetString(Bytes, 0, Bytes.Length);
            }
        }

        public VLTMessage(IPEndPoint iPEndPoint) 
        {
            this.IPEndPoint = iPEndPoint;
        }

        public VLTMessage(byte[] bytes, IPEndPoint iPEndPoint) : this(iPEndPoint) => this.Bytes = bytes;

        public VLTMessage(string message, int length, IPEndPoint iPEndPoint) : this(iPEndPoint) => 
            this.Bytes = ByteGetter.GetStringBytes(message, length);
        
    }
}
