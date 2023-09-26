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

        // Handles incoming chat message
        private async Task HandleIncomingChatMessage((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Tries to cast the incoming message to ChatMessage. If it fails, chatMessage will be null.
            ChatMessage chatMessage = messageInfo.Message as ChatMessage;

            //Chat chat = new() { ChatMessage = chatMessage, ChatPlayerName = playerManager.players[playerID].name, Time = DateTime.Now };

            // Checks if the cast was successful
            if(chatMessage != null)
            {
                // Logs the chat message
                Console.WriteLine($"{chatMessage.Time} - {chatMessage.UserName}: {chatMessage.Message}");

                // Checks if the message has high priority
                if(messageInfo.Priority == MessagePriority.High)
                {
                    // Sends an acknowledgment for high-priority messages
                    // TODO: ACKID await MessageSender.SendAcknowledgment(playerID, MessageType.ChatMessage, messageId);
                }
            }

           //await MessageSender.SendDataToClients(chat);
            await Task.CompletedTask;
        }

       


        private async Task HandleIncomingChatCommand((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Deserialize the incoming data into a NetworkMessageCommands object
            ChatCommand chatCommand = messageInfo.Message as ChatCommand;

            // Handle the command
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
                    break;
                case Commands.All:
                    // Broadcast message to all clients
                    // TODO: MESSAGESENDER SendMessageToAllClients BroadcastMessage($"{clientName}: {messageCommands.Message}");
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
                    break;
                default:
                    Console.WriteLine("Unknown command received.");
                    break;
            }

            await Task.CompletedTask;
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

            await Task.CompletedTask;
        }
    }
}
