using NetCoreServer;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using WebCamViewer2_Server.Stream;
using Stream = WebCamViewer2_Server.Stream.Stream;
using TcpClient = NetCoreServer.TcpClient;
using UdpClient = System.Net.Sockets.UdpClient;

namespace WebCamViewser2_Client
{
    public class StreamClient : TcpClient
    {
        private readonly Stream stream;

        private Thread? queueThread;
        private ConcurrentQueue<byte[]> queue = new();

        private long skipped = 0;
        private long dequeued = 0;
        private long delay = 0;

        public StreamClient(Stream stream) : base(stream.serverAddress, stream.serverPort)
        {
            this.stream = stream;

            this.queueThread = new Thread(async () =>
            {
                while (true)
                {
                    if (queue.TryDequeue(out byte[]? data))
                    {
                        await stream.streamClient.SendAsync(data, stream.streamAddress);
                    }
                }
            });
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
                Console.WriteLine($"Camera: {stream.streamId} Skipped: {dequeued} frames with {skipped}bytes");
                delay = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 5000;
                skipped = 0;
                dequeued = 0;
            }
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Stream: {this.stream.streamId} -> Is connected.");
            if (queueThread != null)
            {
                Console.WriteLine($"Stream: {stream.streamId} Starting queue server");
                queueThread.Start();
            }
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Stream: {this.stream.streamId} -> Is disconnected.");
            if (queueThread != null)
            {
                Console.WriteLine($"Stream: {stream.streamId} Stopping queue server");
                queueThread.Interrupt();
            }
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            try
            {
                int packetSize = 65507;
                if (size > packetSize)
                {
                    //Console.WriteLine($"Buffer: {buffer.Length} Offset: {offset} Size: {size}");
                    byte[] bytes;

                    long reading = 0;
                    long position = 0;
                    while (position < buffer.Length)
                    {
                        bytes = new byte[packetSize];
                        reading = position + packetSize > buffer.Length ? buffer.Length - position : packetSize;
                        //Console.WriteLine("Buffer1: " + buffer.Length + " Reading: " + reading + " Position: " + position);
                        Array.Copy(buffer, position, bytes, 0, reading);
                        position += reading;
                        //Console.WriteLine("Buffer2: " + buffer.Length + " Reading: " + reading + " Position: " + position);

                        Queue(bytes);
                        //stream.streamClient.SendAsync(bytes, stream.streamAddress);
                    }
                }
                else
                {
                    byte[] bytes = new byte[size];
                    Array.Copy(buffer, bytes, size);
                    Queue(bytes);
                    //stream.streamClient.SendAsync(bytes, stream.streamAddress);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Stream: {stream.streamId} -> Caught an error with code {error}.");
        }
    }
}
