using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebCamViewser2_Client;

namespace WebcamViewer.camera
{
    public class GoProCamera : IDisposable
    {
        private static IPEndPoint GoProStreamAddress = IPEndPoint.Parse("0.0.0.0:8554");

        public static List<string> GetCameraIpAddress()
        {
            List<string> result = new List<string>();

            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);

            foreach (IPAddress iPAddress in hostEntry.AddressList)
            {
                if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    byte[] bytes = iPAddress.GetAddressBytes();
                    if (bytes[0] == 172
                        && bytes[1] >= 20
                        && bytes[1] <= 29
                        && bytes[3] >= 50
                        && bytes[3] <= 70)
                    {
                        string validAddress = iPAddress.ToString();
                        result.Add($"{validAddress[..^1]}1");
                    }
                }
            }

            return result;
        }

        private readonly string ipAddress;

        private readonly HttpClient httpClient = new();

        private Thread? streamServer;
        private Thread? queueThread;
        private UdpClient? client;
        //        private UdpClient? clientSender;
        private StreamClient? clientSender;
        private ConcurrentQueue<byte[]> queue = new();

        public GoProCamera(string ipAddress)
        {
            this.ipAddress = ipAddress;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => this.Dispose();
        }

        public async Task<ResponseStatus> SendRequestAsync(string endpoint)
        {
            try
            {
                string request = $"http://{ipAddress}:8080/gopro/{endpoint}";
                using HttpResponseMessage response = await this.httpClient.GetAsync(request);
                return response.IsSuccessStatusCode
                    ? ResponseStatus.Success
                    : ResponseStatus.Failed;
            }
            catch (Exception ex)
            {
                //TODO log exception
                Console.WriteLine(ex);
                return ResponseStatus.Failed;
            }
        }

        public async Task<ResponseStatus> SetSettingAsync(int id, int value)
        {
            return await SendRequestAsync($"camera/setting?setting={id}&option={value}");
        }

        public void StartStreamServer(IPEndPoint streamAddress)
        {
            if (this.streamServer == null)
            {
                this.client = new(new IPEndPoint(IPAddress.Any, 8554));
                //this.clientSender = new(0, streamAddress.AddressFamily);
                this.clientSender = new(streamAddress);
                this.clientSender.Connect();

                IPEndPoint? goproAddress = null;

                Console.WriteLine("Sending gopro config...");
                //this.SendRequestAsync("camera/setting?setting=2&option=1").Wait();
                //this.SendRequestAsync("camera/setting?setting=3&option=5").Wait();
                //this.SendRequestAsync("camera/setting?setting=59&option=0").Wait();
                //this.SendRequestAsync("camera/setting?setting=134&option=2").Wait();
                //this.SendRequestAsync("camera/setting?setting=173&option=0").Wait();
                this.SendRequestAsync("webcam/stop").Wait();
                Console.WriteLine("GoPro stream stopped, starting in 2 seconds");
                Thread.Sleep(2000);
                Console.WriteLine("GoPro stream starting...");
                this.SendRequestAsync("webcam/start?res=12&fov=0").Wait();
                Console.WriteLine("GoPro stream started");

                this.streamServer = new Thread(() =>
                {
                    long skipped = 0;
                    long dequeued = 0;
                    long delay = 0;
                    while (true)
                    {
                        byte[] data = this.client.Receive(ref goproAddress);
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
                            Console.WriteLine($"Skipped: {dequeued} frames with {skipped}bytes");
                            delay = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 5000;
                            skipped = 0;
                            dequeued = 0;
                        }
                    }
                });
                this.queueThread = new Thread(async () =>
                {
                    long keepAliveDelay = 0;
                    while(true)
                    {
                        if (keepAliveDelay < DateTimeOffset.Now.ToUnixTimeMilliseconds())
                        {
                            Console.WriteLine("Sending keep-alive");
                            await SendRequestAsync("camera/keep_alive");
                            keepAliveDelay = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (1000 * 60 * 2);
                        }
                        if (queue.TryDequeue(out byte[]? data))
                        {
                            clientSender.Send(data);
                        }
                    }
                });

                this.queueThread.Start();
                this.streamServer.Start();
            }
        }

        public bool StopStreamServer()
        {
            if (this.streamServer != null)
            {
                this.SendRequestAsync("webcam/stop").Wait();
                this.streamServer.Interrupt();
                this.streamServer = null;
                return true;
            }
            if (this.queueThread != null)
            {
                this.queueThread.Interrupt();
                this.queueThread = null;
            }
            return false;
        }

        public void Dispose()
        {
            this.StopStreamServer();
            this.httpClient?.Dispose();
            this.client?.Dispose();
            this.clientSender?.Dispose();
        }
    }
}
