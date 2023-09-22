using System;
using System.Collections.Concurrent;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using SharedData;

namespace GameServer
{
    // Klasse til at håndtere klientrelateret logik
    public class ClientManager
    {
        // TODO: Clientmanager som singleton, med mindre det er tråd usikkert
        public ConcurrentDictionary<byte, IPEndPoint> clients = new ConcurrentDictionary<byte, IPEndPoint>();
        private byte nextAvailableID = 0;
        private Queue<byte> reusableIDs = new Queue<byte>();
        private readonly object lockObjectForID = new object();

        

        public ClientManager()
        {
            
        }

        /// <summary>
        /// Find byte id på klienten ved hjælp af ip.
        /// Bruger queue til at genbruge numre
        /// </summary>
        /// <param name="clientEndPoint"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte DeterminePlayerID(IPEndPoint clientEndPoint)
        {
            // Sikrer, at kun én tråd ad gangen kan udføre denne kodeblok.
            lock(lockObjectForID)
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

        // Få ID fra IPEndPoint
        public byte GetIDFromIPEndPoint(IPEndPoint clientEndPoint)
        {
            return clients.FirstOrDefault(entry => entry.Value.Equals(clientEndPoint)).Key;
        }

        // 
        public ConcurrentDictionary<byte, IPEndPoint> GetClients()
        {
            return clients; // Return the existing ConcurrentDictionary of clients
        }


        // Metode til at håndtere uventet frakobling af klient
        public async Task HandleClientLeftUnexpectedly(SocketException e, IPEndPoint clientEndPoint)
        {
            Console.WriteLine("Player Left Unexpected");

            // Finder ID'et forbundet med den frakoblede klient.
            byte keyOfLeftUser = GetIDFromIPEndPoint(clientEndPoint);

            // Fjerner spillerens IPEndPoint fra klientlisten
            if(clients.TryRemove(keyOfLeftUser, out IPEndPoint removedEndPoint))
            {
                // TODO : Handling når en bruger er fjernet
            }

            Console.WriteLine($"removed client : {clientEndPoint}");

            // Sender en besked til de andre klienter for at informere dem om frakoblingen.
            PlayerLeft playerLeftMessage = new PlayerLeft { playerID = keyOfLeftUser };

            // TODO: FIX DENNE !! await SendDataToClients(playerLeftMessage, MessageType.PlayerLeft, MessagePriority.Low);            

            Console.WriteLine("send player left message to rest of Clients");

            // Nulstiller clientEndPoint for at frigive ressourcer.
            clientEndPoint = null;

            Console.WriteLine($"exception : {e}");

            // Opdaterer spilverdenens tilstand.
            //world.PlayerLeft(keyOfLeftUser);
        }
    }
}