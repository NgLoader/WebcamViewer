using NetCoreServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCamViewer2_Server.Stream
{
    public class StreamClientRecieve : TcpServer
    {
        internal readonly Stream stream;

        public StreamClientRecieve(Stream stream) : base(IPAddress.Any, stream.streamPort)
        {
            this.stream = stream;
        }

        protected override TcpSession CreateSession()
        {
            return new StreamClientRecieveSession(this);
        }
    }
}
