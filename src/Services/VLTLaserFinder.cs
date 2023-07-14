using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace VLTLaserControllerNET.Services
{
    public class VLTLaserFinder
    {
        private UdpClient UdpSend { get; set; } = new UdpClient();
        private IPEndPoint SendEndPoint { get; set; }

        private UdpClient UdpReciver { get; set; }
        private IPEndPoint RecivedEndPoint { get; set; }


        public static bool PingDevice(IPAddress iPAddress)
        {
            Ping p = new Ping();
            return p.Send(iPAddress).Status == IPStatus.Success;
        }
    }
}
