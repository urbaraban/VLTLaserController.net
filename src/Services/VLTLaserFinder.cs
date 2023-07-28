using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;

namespace VLTLaserControllerNET.Services
{
    public class VLTLaserFinder
    {
        private UdpClient UdpSend { get; set; } = new UdpClient();
        private IPEndPoint SendEndPoint { get; set; }

        public static bool PingDevice(IPAddress iPAddress)
        {
            var p = new Ping();
            return p.Send(iPAddress).Status == IPStatus.Success;
        }
    }
}
