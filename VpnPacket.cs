namespace VpnProtocol
{
    public struct VpnPacket
    {
        public ushort Version { get; set; }   // Версия протокола
        public ushort Flags { get; set; }      // Флаги (зарезервировано)
        public uint SessionId { get; set; }    // Идентификатор сессии
        public long PacketId { get; set; }     // Уникальный ID пакета
        public byte[] Payload { get; set; }    // Полезная нагрузка (IP-пакет)

        public VpnPacket(uint sessionId, long packetId, byte[] payload)
        {
            Version = ProtocolConstants.ProtocolVersion;
            Flags = 0;
            SessionId = sessionId;
            PacketId = packetId;
            Payload = payload;
        }
    }
}