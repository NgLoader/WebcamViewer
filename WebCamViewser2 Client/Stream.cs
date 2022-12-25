using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WebCamViewser2_Client;

namespace WebCamViewer2_Server.Stream
{
    public class Stream : IDisposable
    {
        private static volatile int ID = 0;

        internal protected readonly int streamId = Interlocked.Increment(ref ID);

        internal readonly string serverAddress;
        internal readonly int serverPort;
        internal readonly int streamPort;

        internal readonly StreamClient clientServer;

        internal readonly UdpClient streamClient;
        internal readonly IPEndPoint streamAddress;

        public Stream(string serverAddress, int serverPort, int streamPort)
        {
            this.serverAddress = serverAddress;
            this.serverPort = serverPort;
            this.streamPort = streamPort;

            this.streamAddress = new IPEndPoint(IPAddress.Loopback, this.streamPort);

            this.clientServer = new StreamClient(this);
            this.streamClient = new UdpClient(0, this.streamAddress.AddressFamily);

            AppDomain.CurrentDomain.ProcessExit += (s, e) => this.Dispose();
        }

        public void Start()
        {
            this.clientServer.ConnectAsync();
        }

        public void Stop()
        {
            this.clientServer?.DisconnectAsync();
        }

        public void Dispose()
        {
            this.streamClient?.Dispose();
            this.clientServer?.Dispose();
        }
    }
}
