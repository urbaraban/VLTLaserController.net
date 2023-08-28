using System.Linq;
using System;
using VLTLaserControllerNET.Services;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace VLTLaserControllerNET
{
    public class VLTSender : UdpConnector
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        public int SendSpeedKbit => 2048 * (VLTLaserInfo.WebServer == true ? 4 : 8);

        public VLTLaserINFO VLTLaserInfo { get; private set; }


        public void UpdateInfo()
        {
            VLTMessage request = SendCommand("INFO", ReciveTimeout);
            this.VLTLaserInfo = new VLTLaserINFO(request.Message);
        }

        public void Connect()
        {
            if (this.UdpReciver != null)
            {
                this.Disconnect();
            }

            SendEndPoint = new IPEndPoint(IPAddress, 5011);

            this.RecivedEndPoint = new IPEndPoint(IPAddress.Any, 5011);
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


        public async void SendFrame(byte[] frame)
        {
            //if (true)
            if (this.VLTLaserInfo != null)
            {
                int bytePerSecond = (int)1 * 125000;
                double microsecondbyte = 1000000.0 / bytePerSecond; ;

                int packetLength = 1458;
                int sessionLength = VLTLaserInfo.WebServer == false ? 10 : 5;
                int sessionsize = packetLength * sessionLength;

                int frameSessionCount = (int)Math.Ceiling((double)frame.Length / (packetLength * sessionLength));
                for (int i = 0; i < frameSessionCount; i += 1)
                {
                    int sessinactualsize = Math.Min(sessionsize, frame.Length - sessionsize * i);
                    for (int j = 0; j < sessinactualsize; j += packetLength)
                    {
                        int skip = i * sessionsize + j;
                        byte[] packet = frame.Skip(skip).Take(packetLength).ToArray();
                        SendBytes(packet);
                        udelay((int)(packet.Length * microsecondbyte));
                    }
                    if (WaitBytes(ReciveTimeout).Message.StartsWith("act") == false)
                    {
                        return;
                    }
                }
                udelay((int)(1000 * microsecondbyte));
            }
        }

        protected VLTMessage SendCommand(string command, byte[] args, int timeoutrequest = 0)
        {
            if (command.Length + args.Length < 7)
            {
                byte[] bytes = ByteGetter.GetStringBytes(command, 6);
                for (int i = 0; i < args.Length; i += 1)
                {
                    bytes[command.Length + i] = args[i];
                }
                return SendCommand(bytes, timeoutrequest);
            }
            return new VLTMessage();
        }

        protected VLTMessage SendCommand(string command, int timeoutrequest = 0)
        {
            if (string.IsNullOrEmpty(command) == false && command.Length < 7)
            {
                byte[] bytes = ByteGetter.GetStringBytes(command, 6);
                return SendCommand(bytes, timeoutrequest);
            }
            return new VLTMessage();
        }

        protected VLTMessage SendCommand(byte[] data, int timeoutrequest = 0)
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

        private static void udelay(long us)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            long v = (us * System.Diagnostics.Stopwatch.Frequency) / 1000000;
            while (sw.ElapsedTicks < v) ; ;
        }
    }
}
