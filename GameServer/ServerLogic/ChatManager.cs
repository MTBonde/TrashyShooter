using System.Collections.Concurrent;
using System.Net;

using SharedData;

namespace GameServer
{
    public class ChatManager
    {
        public delegate Task MessageHandlerDelegate((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID);

        // Ordbog til at mappe MessageType til den tilsvarende beskedhåndterer
        private Dictionary<MessageType, MessageHandlerDelegate> chatMessageHandlers;

        private ClientManager clientManager;
        private PlayerManager playerManager;

        public ChatManager(ClientManager clientManager, PlayerManager playerManager)
        {
            this.clientManager = clientManager;
            this.playerManager = playerManager;

            chatMessageHandlers = new Dictionary<MessageType, MessageHandlerDelegate>
            {
                { MessageType.ChatMessage, HandleIncomingChatMessage },
                { MessageType.ChatCommand, HandleIncomingChatCommand },
                { MessageType.ChatAcknowledgement, HandleIncomingChatAcknowledgement }
            };
        }

        public async Task HandleIncomingMessage(byte[] receivedData, byte playerID)
        {
            (NetworkMessage message, MessageType messageType, MessagePriority messagePriority) = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);

            if(chatMessageHandlers.TryGetValue(messageType, out MessageHandlerDelegate handler))
            {
                Console.WriteLine($"Received message of type {messageType} from player {playerID}");

                // Process the message
                await handler((message, messageType, messagePriority), playerID);
            }
        }


        // Handles incoming chat message
        private async Task HandleIncomingChatMessage((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Attempt to cast the incoming NetworkMessage to a ChatMessage type.
            ChatMessage chatMessage = messageInfo.Message as ChatMessage;

            // If the cast is successful, then set the properties.
            if(chatMessage != null)
            {
                // TODO WHERE GET STRING? chatMessage.Message =
                chatMessage.UserName = playerManager.players[playerID].name;
                chatMessage.Time = DateTime.Now;

                // Output the chat message to the console.
                Console.WriteLine($"{chatMessage.Time} - {chatMessage.UserName}: {chatMessage.Message}");

                // Asynchronously send the chat data to all connected clients.
                await MessageSender.SendDataToClients(chatMessage, MessageType.ChatMessage, MessagePriority.High);

                // If this is a high-priority message, track it for acknowledgment.
                if(messageInfo.Priority == MessagePriority.High)
                {
                    MessageSender.TrackMessage(chatMessage, MessageType.ChatMessage, MessagePriority.High, null /*clientEP*/, playerID);
                }
            }
        }




        private async Task HandleIncomingChatCommand((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Deserialize the incoming data into a NetworkMessageCommands object           
            ChatCommand chatCommand = messageInfo.Message as ChatCommand;
            if(chatCommand != null)
            {
                string clientName = playerManager.players[playerID].name;
                switch(chatCommand.Command)
                {
                    case Commands.List:
                        //List<string> clientNamesList = new List<string>();
                        //foreach(var clientEntry in clients)
                        //{
                        //    clientNamesList.Add(clientEntry.Value.Name);
                        //}
                        //string clientNames = string.Join(", ", clientNamesList);
                        //// TODO: MESSAGSENDER SendDirectMessage(client, $"Connected clients: {clientNames}");

                        var clientNamesList = playerManager.players.Values.Select(p => p.name).ToList();
                        string clientNames = string.Join(", ", clientNamesList);
                        // Replace with your real message sending logic
                        // await MessageSender.SendDirectMessage(client, $"Connected clients: {clientNames}");
                        break;
                    case Commands.All:
                        // Broadcast message to all clients
                        await MessageSender.SendDataToClients(chatCommand, MessageType.ChatCommand, MessagePriority.High);
                        break;
                    case Commands.Direct:

                        //foreach(var clientEntry in clients)
                        //{
                        //    // TODO: PLAYER/CLIENT ID NAME IP
                        //    if(clientEntry.Value.Name == chatCommand.TargetName)
                        //    {
                        //        targetClient = clientEntry.Value.TcpClient;
                        //        break;
                        //    }
                        //}

                        //if(targetClient != null)
                        //{
                        //    // TODO: MESSAGESENDER SendDirectMessage(targetClient, $"{clientName}: {messageCommands.Message}");
                        //}
                        //else
                        //{
                        //    // TODO: MESSAGESENDER  SendDirectMessage(client, "Target client not found.");
                        //}
                        
                        //Player targetPlayer = playerManager.players[playerID].name;
                        //if(targetPlayer != null)
                        //{
                        //    // await MessageSender.SendDirectMessage(targetPlayer.TcpClient, $"{clientName}: {chatCommand.Message}");
                        //}
                        //else
                        //{
                        //    // await MessageSender.SendDirectMessage(client, "Target client not found.");
                        //}
                        break;
                    default:
                        Console.WriteLine("Unknown command received.");
                        break;
                }
            }
        }

        private async Task HandleIncomingChatAcknowledgement((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Try to cast the incoming message to ChatAcknowledgement. If it fails, chatAck will be null.
            ChatAcknowledgement chatAck = messageInfo.Message as ChatAcknowledgement;

            // Check if the cast was successful
            if(chatAck != null)
            {
                // Log the received acknowledgment
                Console.WriteLine($"Received chat acknowledgement from player {playerID} for original message type {chatAck.OriginalMessageType}.");

                // TODO: CHAT what else??
            }

            // Henter alle klienter fra ClientManager
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();

            // Finder klientens IPEndPoint fra ConcurrentDictionary
            if(clients.TryGetValue(playerID, out IPEndPoint thisClientEndPoint))
            {
                // Sender svar til klienten
                await MessageSender.SendAsync(chatAck, MessageType.ChatAcknowledgement, MessagePriority.High, thisClientEndPoint);
            }
        }
    }
}
