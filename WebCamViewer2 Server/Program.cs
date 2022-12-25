using NetCoreServer;
using System;
using System.Runtime.CompilerServices;
using Stream = WebCamViewer2_Server.Stream.Stream;

public class Program
{
    private static readonly List<Stream> _streams = new();

    private static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("Commands: create, delete, select, list, exit");

            string? consoleInput = Console.ReadLine();
            if (consoleInput == "create")
            {
                Console.WriteLine("Please enter your stream reacive port:");
                consoleInput = Console.ReadLine();

                if (consoleInput != null && int.TryParse(consoleInput, out int streamPort))
                {
                    Console.WriteLine("Please enter your client server port:");
                    consoleInput = Console.ReadLine();

                    if (consoleInput != null && int.TryParse(consoleInput, out int clientPort))
                    {
                        Stream stream = new(clientPort, streamPort);
                        _streams.Add(stream);

                        Console.WriteLine($"Successful created stream instance. (StreamId: {stream.streamId})");
                    }
                    else
                    {
                        Console.WriteLine("Please enter a valid client port!");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter a valid stream port!");
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
                    Console.WriteLine($" - [{stream.streamId}] Stream Recieve Port: {stream.streamPort} Client Server Port: {stream.clientPort}");
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
                Console.WriteLine("Status client server started: " + stream.clientServer.IsStarted);
                Console.WriteLine("");
                Console.WriteLine("Stream recieve port: " + stream.streamPort);
                Console.WriteLine("Server client port: " + stream.clientPort);
                Console.WriteLine("");
                Console.WriteLine("Connected clients: " + stream.clientServer.ConnectedSessions);
                Console.WriteLine("Sended bytes: " + stream.clientServer.BytesSent);
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