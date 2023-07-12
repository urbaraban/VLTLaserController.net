using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using VLTLaserControllerNET.Services;

namespace VLTLaserControllerNET
{
    public class VLTLaserController
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public bool IsAlive => PingDevice();
        public bool IsConnected => WakeUpDevice();
        public bool IsPlay { get; set; } = false;
        public bool IsAutoturn { get; private set; }
        public int AutoturnTimeout { get; private set; }

        public IPAddress IPAddress { get; set; } = new IPAddress(new byte[] { 192, 168, 1, 1 });
        public int SendPort { get; set; }
        public int ReceivePort { get; private set; }
        public int ReciveTimeout { get; set; } = 500;

        private UdpClient UdpSend { get; set; } = new UdpClient();
        private IPEndPoint SendEndPoint { get; set; }

        private UdpClient UdpReciver { get; set; }
        private IPEndPoint RecivedEndPoint { get; set; }

        public VLTLaserController(IPAddress address, int port = 5011)
        {
            this.IPAddress = address;
            this.SendPort = port;
        }

        // Необходимо обеспечивать последовательное соединение проекторов и наче проекторы будут слать все на один порт
        public void Connect()
        {
            SendEndPoint = new IPEndPoint(IPAddress, SendPort);

            this.RecivedEndPoint = new IPEndPoint(IPAddress.Any, SendPort);
            this.UdpReciver = new UdpClient(RecivedEndPoint);

            VLTMessage request = SendCommand("LINK", 1, ReciveTimeout);

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
                    Connected?.Invoke(this, new EventArgs());
                }
            }
            else
            {
                UdpReciver.Close();
                UdpReciver.Dispose();
            }
        }

        public void Disconnect()
        {
            VLTMessage request = SendCommand("LINK", 0, ReciveTimeout);

            if (request.Message.StartsWith("dis") == true)
            {
                this.IsAutoturn = request.Bytes[3] == 0x01;
                this.AutoturnTimeout = int.Parse(request.Bytes[4].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
            }
        }

        public void Reset()
        {
            SendCommand("RESET", ReciveTimeout);
            while (this.IsAlive == false) { }
            this.UdpReciver.Close();
            this.UdpReciver.Dispose();
            this.Connect();
        }

        private VLTMessage SendCommand(string command, byte arg, int timeoutrequest = 0)
        {
            byte[] bytes = ByteGetter.GetStringBytes(command, 6);
            bytes[command.Length] = arg;
            return SendCommand(bytes, timeoutrequest);
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
            if (timeout > 0 && UdpReciver.Client != null)
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

        private bool PingDevice()
        {
            Ping p = new Ping();
            return p.Send(this.IPAddress).Status == IPStatus.Success;
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
