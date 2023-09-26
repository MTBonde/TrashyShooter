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


        // Håndterer indgående chatbeskeder
        private async Task HandleIncomingChatMessage((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Forsøg at caste den indgående NetworkMessage til en ChatMessage type
            ChatMessage chatMessage = messageInfo.Message as ChatMessage;

            // Hvis castet er succesfuldt, så sæt egenskaberne
            if(chatMessage != null)
            {
                // TODO: hvor beskedindholdet kommer fra`??
                // For nuværende sætter vi brugernavnet og tidspunktet for beskeden
                chatMessage.UserName = playerManager.players[playerID].name;
                chatMessage.Time = DateTime.Now;

                // Udskriv chatbeskeden til konsollen
                Console.WriteLine($"{chatMessage.Time} - {chatMessage.UserName}: {chatMessage.Message}");

                // Send chatdataen asynkront til alle tilsluttede klienter
                await MessageSender.SendDataToClients(chatMessage, MessageType.ChatMessage, MessagePriority.High);

                // Hvis dette er en høj-prioritets besked, så spore den for kvittering
                if(messageInfo.Priority == MessagePriority.High)
                {
                    // Hent alle klienter fra ClientManager
                    ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
                    // Find endpoint for den klient, der lavede anmodningen
                    if(clients.TryGetValue(playerID, out IPEndPoint clientEP))
                    {
                        // Spor beskeden for kvittering. 
                        MessageSender.TrackMessage(chatMessage, MessageType.ChatMessage, MessagePriority.High, clientEP, playerID);
                    }
                }
            }
        }


        // Funktion til at håndtere indgående chatkommandoer
        private async Task HandleIncomingChatCommand((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Deserialiser den indkommende besked til et ChatCommand-objekt         
            ChatCommand chatCommand = messageInfo.Message as ChatCommand;
            if(chatCommand != null)
            {
                // Hent navnet på den klient, der sendte anmodningen
                string clientName = playerManager.players[playerID].name;

                // Hent alle klienter fra ClientManager
                ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();

                // Find endpoint for den klient, der lavede anmodningen
                if(clients.TryGetValue(playerID, out IPEndPoint clientEP))

                    switch(chatCommand.Command)
                    {
                        case Commands.List:

                            // Opret en liste over alle klientnavne fra playerManager
                            List<string> clientNamesList = playerManager.players.Values.Select(p => p.name).ToList();

                            // Konverter listen til en kommasepareret streng
                            string clientNames = string.Join(", ", clientNamesList);

                            // Opret en ny ChatCommand objekt til at sende klientnavnelisten
                            ChatCommand listCommand = new ChatCommand
                            {
                                Command = Commands.List,
                                UserName = clientName, // Name of the client who requested the list
                                Time = DateTime.Now,
                                Message = clientNames // List of client names
                            };

                            // Send beskeden til den klient, der anmodede om det
                            await MessageSender.SendAsync(listCommand, MessageType.ChatCommand, MessagePriority.Low, clientEP);
                            break;
                        case Commands.All:
                            // Broadcast til alle
                            await MessageSender.SendDataToClients(chatCommand, MessageType.ChatCommand, MessagePriority.High);
                            break;
                        case Commands.Direct:
                            // Initialiser variabel til at opbevare klientens endpoint
                            IPEndPoint targetClientEP = null;

                            // Søg efter klienten
                            foreach(KeyValuePair<byte, IPEndPoint> entry in clients)
                            {
                                PlayerInfo playerInfo = playerManager.players[entry.Key];
                                if(playerInfo.name == chatCommand.TargetName)
                                {
                                    targetClientEP = entry.Value;
                                    break;
                                }
                            }

                            if(targetClientEP != null)
                            {
                                ChatCommand directMessage = new ChatCommand
                                {
                                    Command = Commands.Direct,
                                    UserName = clientName,
                                    Time = DateTime.Now,
                                    TargetName = chatCommand.TargetName,
                                    Message = chatCommand.Message
                                };

                                await MessageSender.SendAsync(directMessage, MessageType.ChatCommand, MessagePriority.High, targetClientEP);
                            }
                            else
                            {
                                ChatCommand errorMessage = new ChatCommand
                                {
                                    Command = Commands.Direct,
                                    UserName = "Server",
                                    Time = DateTime.Now,
                                    Message = "Target client not found."
                                };

                                await MessageSender.SendAsync(errorMessage, MessageType.ChatCommand, MessagePriority.Low, clientEP);
                            }
                            break;

                        default:
                            Console.WriteLine("Unknown command received.");
                            break;
                    }
            }
        }

        private async Task HandleIncomingChatAcknowledgement((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Forsøg at caste den indgående besked til ChatAcknowledgement. Hvis det mislykkes, vil chatAck være null.
            ChatAcknowledgement chatAck = messageInfo.Message as ChatAcknowledgement;

            // Tjek om castet var succesfuldt
            if(chatAck != null)
            {
                // Log den modtagne bekræftelse
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
