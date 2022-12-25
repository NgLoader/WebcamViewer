using System.Net;
using WebCamViewer2_Server.Stream;
using Stream = WebCamViewer2_Server.Stream.Stream;

internal class Program
{
    private static readonly List<Stream> _streams = new();
     
    private static void Main(string[] args)
    {
        Console.WriteLine("GoProTunnel Client - v0.0.1");

        while (true)
        {
            Console.WriteLine("Commands: create, delete, select, list, exit");

            string? consoleInput = Console.ReadLine();
            if (consoleInput == "create")
            {
                Console.WriteLine("Please enter your server address:");
                consoleInput = Console.ReadLine();

                if (consoleInput != null && IPAddress.TryParse(consoleInput, out IPAddress? address))
                {
                    Console.WriteLine("Please enter your server port:");
                    consoleInput = Console.ReadLine();

                    if (consoleInput != null && int.TryParse(consoleInput, out int serverPort))
                    {
                        Console.WriteLine("Please enter your local stream server port:");
                        consoleInput = Console.ReadLine();

                        if (consoleInput != null && int.TryParse(consoleInput, out int streamPort))
                        {
                            Stream stream = new(address.ToString(), serverPort, streamPort);
                            _streams.Add(stream);

                            Console.WriteLine($"Successful created stream instance. (StreamId: {stream.streamId})");
                        }
                        else
                        {
                            Console.WriteLine("Please enter a valid stream server port!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid client server port!");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a valid client server address!");
                }
            }
            else if (consoleInput == "delete")
            {
                Stream? stream = SelectStreamId();
                if (stream != null)
                {
                    try
                    {
                        Console.WriteLine($"Deleteing stream {stream.streamId}");
                        stream.Stop();
                        stream.Dispose();
                        Console.WriteLine($"Successful deleted stream {stream.streamId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error by deleted stream {stream.streamId}!");
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        _streams.Remove(stream);
                    }
                }
            }
            else if (consoleInput == "select")
            {
                Stream? stream = SelectStreamId();
                if (stream != null)
                {
                    StreamSelected(stream);
                }
            }
            else if (consoleInput == "list")
            {
                Console.WriteLine("[] ----- Streams ----- []");
                foreach (Stream stream in _streams)
                {
                    Console.WriteLine($" - [{stream.streamId}] Client Server Address: {stream.serverAddress} Client Server Port: {stream.serverPort} Stream Port: {stream.streamPort}");
                }
                Console.WriteLine("[] ----- Streams ----- []");
            }
            else if (consoleInput == "exit")
            {
                Environment.Exit(0);
            }
        }
    }

    private static Stream? SelectStreamId()
    {
        Console.WriteLine("Please enter your stream id:");
        string? consoleInput = Console.ReadLine();

        if (consoleInput != null && int.TryParse(consoleInput, out int streamId))
        {
            foreach (Stream stream in _streams)
            {
                if (stream.streamId == streamId)
                {
                    return stream;
                }
            }
        }

        Console.WriteLine("Please enter a valid stream id!");
        return null;
    }

    private static void StreamSelected(Stream stream)
    {
        Console.Clear();

        while (true)
        {
            Console.WriteLine($"StreamId: {stream.streamId}");
            Console.WriteLine("Commands: start, stop, info, back");

            string? consoleInput = Console.ReadLine();
            if (consoleInput == "start")
            {
                try
                {
                    Console.WriteLine("Starting stream...");
                    stream.Start();
                    Console.WriteLine("Started stream.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (consoleInput == "stop")
            {
                Console.WriteLine("Stopping stream");
                try
                {
                    Console.WriteLine("Stopping stream...");
                    stream.Stop();
                    Console.WriteLine("Stopped stream.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else if (consoleInput == "info")
            {
                Console.WriteLine("[] ----- Stream Info ----- []");
                Console.WriteLine($"StreamId: {stream.streamId}");
                Console.WriteLine("");
                Console.WriteLine("Status client server connected: " + stream.clientServer.IsConnected);
                Console.WriteLine("");
                Console.WriteLine("Stream client address: " + stream.serverAddress);
                Console.WriteLine("Stream client port: " + stream.serverPort);
                Console.WriteLine("Server stream port: " + stream.streamPort);
                Console.WriteLine("");
                Console.WriteLine("Received bytes: " + stream.clientServer.BytesReceived);
                Console.WriteLine("[] ----- Stream Info ----- []");
            }
            else if (consoleInput == "back")
            {
                Console.Clear();
                break;
            }
        }
    }
}