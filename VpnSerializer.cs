using System;
using System.IO;
using VpnProtocol.Exceptions;

namespace VpnProtocol
{
    public static class VpnSerializer
    {
        public static byte[] Serialize(VpnPacket packet)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write(packet.Version);
                writer.Write(packet.Flags);
                writer.Write(packet.SessionId);
                writer.Write(packet.PacketId);
                writer.Write(packet.Payload.Length);
                writer.Write(packet.Payload);
                
                return ms.ToArray();
            }
        }

        public static VpnPacket Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                var version = reader.ReadUInt16();
                if (version != ProtocolConstants.ProtocolVersion)
                    throw new ProtocolException($"Unsupported protocol version: {version}");
                
                return new VpnPacket
                {
                    Version = version,
                    Flags = reader.ReadUInt16(),
                    SessionId = reader.ReadUInt32(),
                    PacketId = reader.ReadInt64(),
                    Payload = reader.ReadBytes(reader.ReadInt32())
                };
            }
        }
    }
}