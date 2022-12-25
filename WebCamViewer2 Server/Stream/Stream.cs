using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WebCamViewer2_Server.Stream
{
    public class Stream : IDisposable
    {
        private static volatile int ID = 0;

        internal protected readonly int streamId = Interlocked.Increment(ref ID);

        internal readonly int clientPort;
        internal readonly int streamPort;

        internal readonly StreamClientRecieve streamServer;
        internal readonly StreamClientServer clientServer;

        public Stream(int clientPort, int streamPort)
        {
            this.clientPort = clientPort;
            this.streamPort = streamPort;

            this.streamServer = new StreamClientRecieve(this);
            this.clientServer = new StreamClientServer(this);

            AppDomain.CurrentDomain.ProcessExit += (s, e) => this.Dispose();
        }

        public void Start()
        {
            this.streamServer.Start();
            this.clientServer.Start();
        }

        public void Stop()
        {
            this.streamServer?.Stop();
            this.clientServer?.Stop();
        }

        public void Dispose()
        {
            this.streamServer?.Dispose();
            this.clientServer?.Dispose();
        }
    }
}
