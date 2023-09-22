namespace GameServer
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            // Create a new UDPGameServer instance with port 8080.
            UDPGameServer gameServer = new UDPGameServer(8080);
            // Start the server.            
            await gameServer.StartAsync();


            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        //    // Ask the user if they want to host locally or provide an IP
        //    Console.WriteLine("Do you want to host locally? (y/n)");
        //    string localHost = Console.ReadLine().ToLower();

        //    string ipAddress = "127.0.0.1"; // Default to localhost
        //    if(localHost != "y")
        //    {
        //        do
        //        {
        //            // Ask for the specific IP address and validate it
        //            Console.WriteLine("Enter the IP address to host on:");
        //            ipAddress = Console.ReadLine();
        //        } while(!IsValidIP(ipAddress));
        //    }

        //    // Ask the user for the port number or use the default (42069)
        //    Console.WriteLine("Enter the port number above 1234 (or press Enter for default 42069):");
        //    string portInput = Console.ReadLine();

        //    int port = string.IsNullOrEmpty(portInput) ? 42069 : int.Parse(portInput);

        //    // Initialize and start the game server
        //    // Note: The IP address is not used here. You would need to modify your GameServer constructor to actually use it.
        //    GameServer server = new GameServer(port);
        //    server.Start();

        //}

        //// Using build in IPAdress to verify if  user input is a correctly formatted ipadress.
        //// Out tells we expect an output, but we will not store it further so we YEET it.
        //static bool IsValidIP(string ipAddress) { return IPAddress.TryParse(ipAddress, out IPAddress address); }
    }
}
