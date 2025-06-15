namespace VpnProtocol
{
    public static class ProtocolConstants
    {
        public const int HeaderSize = 16; // Размер заголовка в байтах
        public const ushort ProtocolVersion = 0x0001; // Версия протокола
        public const int MaxPacketSize = 1500; // Максимальный размер пакета
    }
}