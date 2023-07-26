namespace VLTLaserControllerNET.Services
{
    internal class VLTPacketManager
    {
    }

    internal struct VLTPacket
    {
        public byte[] Bytes { get; }
        public PacketType PacketType { get; }

        public bool WaitReqest { get; }
    }

    internal enum PacketType
    {
        Command,
        Frame
    }
}
