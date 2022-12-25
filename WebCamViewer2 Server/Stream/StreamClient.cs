using System.Net;
using System.Net.Sockets;

namespace WebCamViewer2_Server.Stream
{
    public class StreamClient : IDisposable
    {
        private Stream stream;

        private Thread? streamServer;
        private UdpClient? client;

        public StreamClient(Stream stream)
        {
            this.stream = stream;
        }

        public void Start()
        {
            if (this.streamServer == null)
            {
                this.client = new(new IPEndPoint(IPAddress.Any, this.stream.streamPort));

                IPEndPoint? goproAddress = null;

                this.streamServer = new Thread(() =>
                {
                    while (true)
                    {
                        byte[] data = this.client.Receive(ref goproAddress);
                        this.stream.clientServer.Multicast(data);
                    }
                });
                this.streamServer.Start();
            }
        }

        public bool Stop()
        {
            if (this.streamServer != null)
            {
                this.streamServer.Interrupt();
                this.streamServer = null;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            this.Stop();
            this.client?.Dispose();
        }
    }
}
