using System.Net;
using WebcamViewer.camera;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Waiting for cameras...");
        while (true)
        {
            List<string> cameras = GoProCamera.GetCameraIpAddress();
            if (cameras.Count != 0)
            {
                Console.WriteLine("Please select a camera:");

                for (int i = 0; i < cameras.Count(); i++)
                {
                    string camera = cameras[i];
                    Console.WriteLine($"[{i}] {camera}");
                }

                string? consoleInput = Console.ReadLine();
                if (int.TryParse(consoleInput, out var input))
                {
                    if (input >= 0 && cameras.Count > input)
                    {
                        string address = cameras[input];
                        CameraSelected(address);
                    }

                    Console.Clear();
                    Console.WriteLine("Waiting for cameras...");
                }
            }

            Thread.Sleep(1000);
        }
    }

    private static void CameraSelected(string address)
    {
        using GoProCamera camera = new GoProCamera(address);
        while (true)
        {
            Console.WriteLine($"Camera address: {address}");
            Console.WriteLine("Commands: start, stop, back, exit");

            string? input = Console.ReadLine();
            if (input == "start")
            {
                Console.WriteLine("Please enter your remote target address:");

                string? server = Console.ReadLine();
                if (server != null && IPEndPoint.TryParse(server, out var serverAddress))
                {
                    camera.StartStreamServer(serverAddress);

                    Console.WriteLine($"Stream streaming to address {serverAddress}");
                }
                else
                {
                    Console.WriteLine("Please enter a valid address!");
                }
            }
            else if (input == "stop")
            {
                Console.WriteLine(camera.StopStreamServer()
                    ? "Stopped successful"
                    : "Stopped failed");
            }
            else if (input == "back")
            {
                if (camera.StopStreamServer())
                {
                    Console.WriteLine("Stopped stream");
                }
                break;
            }
            else if (input == "exit")
            {
                Environment.Exit(0);
            }
        }
    }
}