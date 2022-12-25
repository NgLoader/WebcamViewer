using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebCamViewer2_Server.Stream
{
    public class StreamClientRecieveSession : TcpSession
    {
        private readonly StreamClientRecieve server;

        public StreamClientRecieveSession(StreamClientRecieve server) : base (server)
        {
            this.server = server;
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"GoPro: {this.server.stream.streamId} Client: {this.Id} -> Is connected.");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"GoPro: {this.server.stream.streamId} Client: {this.Id} -> Is disconnected.");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            try
            {
                byte[] data = new byte[size];
                Array.Copy(buffer, offset, data, 0, size);
                server.stream.clientServer.Queue(buffer.AsSpan((int)offset, (int)size)); //buffer.AsSpan((int)offset, (int)size)
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"GoPro: {this.server.stream.streamId} Client: {this.Id} -> Caught an error with code {error}.");
        }
    }
}
