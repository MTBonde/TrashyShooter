using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameServer
{
    // Main server class 
    public class UDPGameServer
    {
        // UDP client to manage incoming and outgoing data.
        private UdpClient udpServer;
        // IPEndPoint to store client info.
        private IPEndPoint endPoint;

        private MessageHandler messageHandler;

        private ConcurrentDictionary<byte, IPEndPoint> clients = new ConcurrentDictionary<byte, IPEndPoint>();
        private byte nextAvailableID = 0;
        // Låseobjekt til at sikre, at ID-generering er atomar.
        private readonly object lockObject = new object();
        private Queue<byte> reusableIDs = new Queue<byte>();

        // Constructor: Initialize the server.
        public UDPGameServer(int port)
        {
            // Initialize UdpClient and bind it to the given port.
            udpServer = new UdpClient(port);
            // Initialize IPEndPoint to capture client data.
            endPoint = new IPEndPoint(IPAddress.Any, port);

            messageHandler = new MessageHandler(clients);
        }
                
        /// <summary>
        /// Asynchronous method for starting the server
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            Console.WriteLine("Server startet.");
            while(true)
            {
                var result = await udpServer.ReceiveAsync();

                // Tilføj logik for at bestemme spillerens ID baseret på result.RemoteEndPoint eller andre faktorer.
                byte playerID = DeterminePlayerID(result.RemoteEndPoint);

                // Brug MessageHandler til at håndtere den modtagne besked
                await messageHandler.HandleIncomingMessage(result.Buffer, playerID);
            }
        }

        /// <summary>
        /// Find byte id på klienten ved hjælp af ip.
        /// Bruger queue til at genbruge numre
        /// </summary>
        /// <param name="clientEndPoint"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private byte DeterminePlayerID(IPEndPoint clientEndPoint)
        {
            // Sikrer, at kun én tråd ad gangen kan udføre denne kodeblok.
            lock(lockObject)
            {
                // Guard Clause: Hvis klienten allerede findes, returnerer vi deres eksisterende ID og afslutter metoden.
                if(clients.Values.Contains(clientEndPoint))
                {
                    // Hvis klienten allerede findes, anvender vi LINQ til at søge i 'clients' ordbogen for at finde den første nøgleværdi-par,
                    // hvor værdien (IPEndPoint) matcher 'clientEndPoint'. Derefter tager vi nøglen (spillerens ID) fra det fundne par.                    
                    return clients.FirstOrDefault(x => x.Value.Equals(clientEndPoint)).Key;
                }

                // Tildeler det næste tilgængelige ID til den nye klient
                byte newPlayerID = nextAvailableID;

                // Tilføjer den nye klient og deres ID til ordbogen
                clients.TryAdd(newPlayerID, clientEndPoint);

                // Opdaterer det næste tilgængelige ID
                nextAvailableID++;

                // Håndterer udmattelse af ID'er, hvis vi når til den maksimale værdi af byte
                if(nextAvailableID == 0)
                {
                    // Guard Clause: Hvis der er genbrugelige ID'er, brug et og returnér.
                    if(reusableIDs.Count > 0)
                    {
                        return reusableIDs.Dequeue();
                    }

                    // Guard Clause: Hvis vi er nået hertil, har vi udmattet vores ID-pulje. Kast en undtagelse.
                    throw new InvalidOperationException("Ran out of available client IDs");
                }
                // Returnerer det tildelte ID for den nye klient
                return newPlayerID;
            }
        }

        // TODO:  game logic, client management,
    }
}
