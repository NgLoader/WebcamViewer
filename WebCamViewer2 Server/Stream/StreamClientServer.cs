using NetCoreServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCamViewer2_Server.Stream
{
    public class StreamClientServer : TcpServer
    {
        internal readonly Stream stream;
        private ConcurrentQueue<byte[]> queue = new();

        private Thread queueThread;
        private long skipped = 0;
        private long dequeued = 0;
        private long delay = 0;

        public StreamClientServer(Stream stream) : base(IPAddress.Any, stream.clientPort)
        {
            this.stream = stream;

            this.queueThread = new Thread(() =>
            {
                while (true)
                {
                    if (queue.TryDequeue(out byte[]? data))
                    {
                        Multicast(data);
                    }
                }
            });
        }

        protected override void OnStarting()
        {
            base.OnStarting();
            if (queueThread != null)
            {
                Console.WriteLine($"Camera: {stream.streamId} Starting queue server");
                queueThread.Start();
            }
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            if (queueThread != null)
            {
                Console.WriteLine($"Camera: {stream.streamId} Stopping queue server");
                queueThread.Interrupt();
            }
        }

        public void Queue(byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                queue.Enqueue(data);
                if (queue.Count() > 100)
                {
                    if (queue.TryDequeue(out byte[]? dataSkip))
                    {
                        skipped += dataSkip.Count();
                        dequeued++;
                    }
                }
            }

            if (delay < DateTimeOffset.Now.ToUnixTimeMilliseconds() && skipped != 0)
            {
                Console.WriteLine($"Camera: {stream.streamId} Skipped: {dequeued} frames with {skipped} bytes");
                delay = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 5000;
                skipped = 0;
                dequeued = 0;
            }
        }

        protected override TcpSession CreateSession()
        {
            return new StreamClientServerSession(this);
        }
    }
}
