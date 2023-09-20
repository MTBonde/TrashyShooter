using System.Collections.Concurrent;
using System.Net;

using SharedData;

namespace GameServer
{
    public class MessageHandler
    {
        // Delegeret til håndtering af beskeder
        public delegate Task MessageHandlerDelegate(byte[] dataToDeserialize, byte playerID);

        // Ordbog til at mappe MessageType til den tilsvarende beskedhåndterer
        private Dictionary<MessageType, MessageHandlerDelegate> messageHandlers;

        private ConcurrentDictionary<byte, IPEndPoint> clients;

        public MessageHandler(ConcurrentDictionary<byte, IPEndPoint> clients)
        {
            this.clients = clients;
            // Initialiser messageHandlers ordbogen
            messageHandlers = new Dictionary<MessageType, MessageHandlerDelegate>
            {
                { MessageType.ClientHasJoined, HandleClientHasJoined },
                { MessageType.ClientJoinAnswer, HandleClientJoinAnswer },
                // TODO: resten af beskederne
            };
        }

        /// <summary>
        /// Asynchronously handles incoming network messages by decoding them and invoking the appropriate message handler based on the message type.
        /// </summary>
        /// <param name="receivedData">The received byte array containing the message.</param>
        /// <param name="playerID">The ID of the player who sent the message.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>

        public async Task HandleIncomingMessage(byte[] receivedData, byte playerID)
        {
            // Modtager den indkommende netværksbesked og dekoder den til dens bestanddele: selve beskeden, beskedtypen, og beskedprioriteten.
            var (message, messageType, messagePriority) = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);

            // Forsøger at finde en beskedhåndteringsmetode baseret på den dekodede beskedtype.
            // Hvis en metode findes, udføres den asynkront med de modtagne data og player ID.
            if(messageHandlers.TryGetValue(messageType, out MessageHandlerDelegate handler))
            {
                await handler(receivedData, playerID);
            }

            else
            {
                Console.WriteLine($"Ukendt beskedtype: {messageType}");
            }
        }

        // beskedhåndteringsmetoder
        private async Task HandleClientHasJoined(byte[] dataToDeserialize, byte playerID)
        {
            // Her kan du indsætte den faktiske logik for at håndtere en "ClientHasJoined" besked
            Console.WriteLine($"Klient med ID {playerID} har tilsluttet sig.");
            await Task.CompletedTask;
        }

        private async Task HandleClientJoinAnswer(byte[] dataToDeserialize, byte playerID)
        {
            // Her kan du indsætte den faktiske logik for at håndtere en "ClientJoinAnswer" besked
            Console.WriteLine($"Klient med ID {playerID} har modtaget et svar om deltagelse.");
            await Task.CompletedTask;
        }

        // Tilføj flere metoder til at håndtere andre beskedtyper
    }
}
