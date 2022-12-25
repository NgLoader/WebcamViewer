using NetCoreServer;
using System.Net;
using System.Net.Sockets;
using TcpClient = NetCoreServer.TcpClient;

namespace WebCamViewser2_Client
{
    public class StreamClient : TcpClient
    {
        public StreamClient(IPEndPoint stream) : base(stream)
        {
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Stream: GoPro -> Is connected.");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Stream: GoPro -> Is disconnected.");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Stream: GoPro -> Caught an error with code {error}.");
        }
    }
}
