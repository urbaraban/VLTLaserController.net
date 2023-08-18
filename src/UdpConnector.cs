using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using VLTLaserControllerNET.Services;

namespace VLTLaserControllerNET
{
    public class UdpConnector : IDisposable
    {
        protected UdpClient UdpSend { get; set; } = new UdpClient();
        protected IPEndPoint SendEndPoint { get; set; }

        protected UdpClient UdpReciver { get; set; }
        protected IPEndPoint RecivedEndPoint { get; set; }

        public int SendPort { get; set; }
        protected int ReceivePort { get; set; }
        protected int ReciveTimeout { get; set; } = 500;

        public IPAddress IPAddress { get; set; } = new IPAddress(new byte[] { 192, 168, 1, 1 });


        public virtual void Disconnect()
        {
            if (this.UdpReciver != null)
            {
                this.UdpReciver.Close();
                this.UdpReciver.Dispose();
                this.UdpReciver = null;
            }
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        protected void SendBytes(byte[] bytes)
        {
            if (bytes.Length > 0 && SendEndPoint != null)
            {
                UdpSend.Send(bytes, bytes.Length, SendEndPoint);
            }
        }

        protected VLTMessage WaitBytes(int timeout)
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
    }

    public struct VLTMessage
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
