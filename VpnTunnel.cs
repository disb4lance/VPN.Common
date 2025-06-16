using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using VpnProtocol.Exceptions;

namespace VpnProtocol
{
    public abstract class VpnTunnel : IDisposable
    {
        protected UdpClient UdpClient { get; set; }
        protected uint SessionId { get; set; }
        protected long PacketCounter { get; private set; }
        protected bool IsActive { get; private set; }

        public event Action<byte[], IPEndPoint> PacketReceived;
        
        protected VpnTunnel()
        {
            UdpClient = new UdpClient();
            PacketCounter = 0;
        }

        public virtual void Start()
        {
            IsActive = true;
            Task.Run(ReceiveLoop);
        }

        public virtual void Stop()
        {
            IsActive = false;
            UdpClient?.Close();
        }

        public void SendPacket(byte[] ipPacket)
        {
            if (!IsActive) return;

            var vpnPacket = new VpnPacket(
                SessionId,
                ++PacketCounter,
                ipPacket
            );

            var serialized = VpnSerializer.Serialize(vpnPacket);
            SendRawPacket(serialized);
        }

        protected abstract void SendRawPacket(byte[] data);
        
        protected abstract void ReceiveLoop();

        protected void HandleReceivedPacket(byte[] data, IPEndPoint remoteEp)
        {
            try
            {
                var vpnPacket = VpnSerializer.Deserialize(data);
                
                if (vpnPacket.SessionId != SessionId)
                    throw new ProtocolException($"Invalid session ID: {vpnPacket.SessionId}");
                
                // Вызываем событие вместо абстрактного метода
                PacketReceived?.Invoke(vpnPacket.Payload, remoteEp);
            }
            catch (ProtocolException ex)
            {
                Console.WriteLine($"Protocol error: {ex.Message}");
            }
        }

        public virtual void Dispose()
        {
            Stop();
            UdpClient?.Dispose();
        }
    }
}