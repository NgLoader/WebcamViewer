using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WebCamViewer2_Server.Stream
{
    public class StreamClientServerSession : TcpSession
    {
        private readonly StreamClientServer server;

        public StreamClientServerSession(StreamClientServer server) : base (server)
        {
            this.server = server;
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Stream: {this.server.stream.streamId} Client: {this.Id} -> Is connected.");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Stream: {this.server.stream.streamId} Client: {this.Id} -> Is disconnected.");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Stream: {this.server.stream.streamId} Client: {this.Id} -> Caught an error with code {error}.");
        }
    }
}
