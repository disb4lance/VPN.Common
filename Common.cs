// Проект: CustomVPN
// Назначение: Минимальный рабочий VPN с кастомным протоколом поверх UDP с поддержкой базового шифрования.
// Архитектура: Клиент ↔ Сервер через UDP. Протокол сериализует и шифрует пакеты. Без TUN, но с перспективой внедрения.

// Структура решения:
// - CustomVpn.Common: классы пакетов, протокол, шифрование
// - CustomVpn.Server: UDP сервер
// - CustomVpn.Client: UDP клиент

// Файл: VpnPacket.cs (в CustomVpn.Common)

using System.Security.Cryptography;
using System.Text;

namespace CustomVpn.Common
{
    public enum PacketType : byte
    {
        Auth = 1,
        Data = 2,
        KeepAlive = 3
    }

    public class VpnPacket
    {
        public PacketType Type { get; set; }
        public Guid SessionId { get; set; }
        public byte[] Payload { get; set; }

        public byte[] Serialize(string key)
        {
            using var ms = new MemoryStream();
            ms.WriteByte((byte)Type);
            ms.Write(SessionId.ToByteArray(), 0, 16);

            var encrypted = Crypto.Encrypt(Payload, key);
            ms.Write(BitConverter.GetBytes(encrypted.Length), 0, 4);
            ms.Write(encrypted);

            return ms.ToArray();
        }

        public static VpnPacket Deserialize(byte[] data, string key)
        {
            using var ms = new MemoryStream(data);
            var type = (PacketType)ms.ReadByte();

            var sessionBytes = new byte[16];
            ms.Read(sessionBytes);

            var lenBytes = new byte[4];
            ms.Read(lenBytes);
            int len = BitConverter.ToInt32(lenBytes);

            var encrypted = new byte[len];
            ms.Read(encrypted);

            return new VpnPacket
            {
                Type = type,
                SessionId = new Guid(sessionBytes),
                Payload = Crypto.Decrypt(encrypted, key)
            };
        }
    }

    public static class Crypto
    {
        public static byte[] Encrypt(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            aes.Key = key;
            aes.GenerateIV();

            using var ms = new MemoryStream();
            ms.Write(aes.IV);
            using var crypto = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            crypto.Write(data);
            crypto.FlushFinalBlock();
            return ms.ToArray();
        }

        public static byte[] Decrypt(byte[] encryptedData, string password)
        {
            using var aes = Aes.Create();
            var key = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            aes.Key = key;

            using var ms = new MemoryStream(encryptedData);
            var iv = new byte[16];
            ms.Read(iv);
            aes.IV = iv;

            using var crypto = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var result = new MemoryStream();
            crypto.CopyTo(result);
            return result.ToArray();
        }
    }
}
