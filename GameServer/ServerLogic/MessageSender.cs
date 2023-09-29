using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

using SharedData;

namespace GameServer
{
    public static class MessageSender
    {
        private static UdpClient udpServer;
        private static ClientManager clientManager;
        private static SnapshotManager snapshotManager;

        // Dictionary to hold unacknowledged messages
        private static ConcurrentDictionary<Guid, UnacknowledgedMessageInfo> unacknowledgedMessages = new ConcurrentDictionary<Guid, UnacknowledgedMessageInfo>();


        private static object lockObject = new object();

        /// <summary>
        /// klasse der starter den statisk sender klasse
        /// </summary>
        /// <param name="udpServer"></param>
        /// <param name="clientManager"></param>
        /// <param name="snapshotManager"></param>
        public static void Initialize(UdpClient udpServer, ClientManager clientManager, SnapshotManager snapshotManager)
        {
            lock(lockObject)
            {
                if(MessageSender.udpServer == null && MessageSender.clientManager == null && MessageSender.snapshotManager == null)
                {
                    MessageSender.udpServer = udpServer;
                    MessageSender.clientManager = clientManager;
                    MessageSender.snapshotManager = snapshotManager;
                }
                ContinouslySendPlayerSnapShot();
            }

        }

        

        /// <summary>
        /// Asynkron afsender en besked med specefik type og prioritet.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="priority">The priority level of the message.</param>
        /// <param name="clientEP">The endpoint of the client to send the message to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task SendAsync<T>(T message,
                                              MessageType messageType,
                                              MessagePriority priority,
                                              IPEndPoint clientEP) where T : NetworkMessage
        {
            Guid messageId = Guid.NewGuid();

            if(priority == MessagePriority.High)
                message.MessageId = messageId;

            // Bruger NetworkMessageProtocol til at lave en samlet serialiseret netværksbesked
            (byte[] MessageBytes, int Length, IPEndPoint ClientEP) networkMessage = NetworkMessageProtocol.SendNetworkMessage(message,
                                                                                                                              messageType,
                                                                                                                              priority,
                                                                                                                              clientEP);

            // Sender den samlede netværksbesked via UDP
            await udpServer.SendAsync(networkMessage.MessageBytes,
                                      networkMessage.Length,
                                      networkMessage.ClientEP);

            if(priority == MessagePriority.High)
            {
                // Use the ClientManager to get the client list
                ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
                TrackMessage(message, messageType, priority, clientEP, 1);

            }
        }


        /// <summary>
        /// Sender til alle klienter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        public static async Task SendDataToClients<T>(T message, MessageType messageType, MessagePriority priority) where T : NetworkMessage
        {
            // Use the ClientManager to get the client list
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
            foreach(KeyValuePair<byte, IPEndPoint> client in clients)
            {
                await SendAsync(message, messageType, priority, client.Value);
            }
        }

        /// <summary>
        /// Sender til alle undtagen en
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="messageType"></param>
        /// <param name="priority"></param>
        /// <param name="exceptionID"></param>
        /// <returns></returns>
        public static async Task SendDataToClientsExceptOne<T>(T message, MessageType messageType, MessagePriority priority, byte exceptionID) where T : NetworkMessage
        {
            // Use the ClientManager to get the client list
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
            foreach(KeyValuePair<byte, IPEndPoint> client in clients)
            {
                if(client.Key != exceptionID)
                {
                    await SendAsync(message, messageType, priority, client.Value);
                }
            }
        }

        /// <summary>
        /// Notificerer andre klienter, når en spiller deltager i spillet.
        /// </summary>
        /// <param name="playerID">Den deltagerende spillers ID.</param>
        /// <param name="thisClientEndPoint">Den deltagerende spillers ip.</param>
        public static async Task NotifyOtherClients(byte playerID, IPEndPoint thisClientEndPoint)
        {
            // Opretter en PlayerJoined besked
            PlayerJoined playerJoined = new PlayerJoined();

            // Henter alle klienter fra ClientManager
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();

            // Går igennem alle klienter
            foreach(KeyValuePair<byte, IPEndPoint> client in clients)
            {
                // Indstiller playerID for beskeden
                playerJoined.playerID = client.Key;

                // Tjekker om klienten er den samme som den, der netop har tilsluttet
                if(client.Value != thisClientEndPoint)
                {
                    // Sender beskeden til de andre klienter
                    await SendAsync(playerJoined, MessageType.PlayerJoined, MessagePriority.Low, thisClientEndPoint);
                }
            }
        }

        /// <summary>
        /// Sender en bekræftelse
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="originalMessageType"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public static async Task SendAcknowledgment(byte playerID, MessageType originalMessageType, Guid messageId)
        {
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
            // Declare as object to handle different acknowledgment types
            NetworkMessage acknowledgmentMessage;
            if(originalMessageType == MessageType.ChatMessage || originalMessageType == MessageType.ChatCommand)
            {
                // Create a chat-specific acknowledgment message
                acknowledgmentMessage = new ChatAcknowledgement
                {
                    playerID = playerID,
                    OriginalMessageType = originalMessageType,
                };

                // Get the IPEndPoint of the client to whom you want to send the acknowledgment
                if(clients.TryGetValue(playerID, out IPEndPoint clientEndPoint))
                {
                    // send the acknowledgment back to the client
                    await SendAsync((ChatAcknowledgement)acknowledgmentMessage, MessageType.ChatAcknowledgement, MessagePriority.low, clientEndPoint);
                }
            }
            else
            {
                // Create a generic acknowledgment message
                acknowledgmentMessage = new Acknowledgement
                {
                    playerID = playerID,
                    OriginalMessageType = originalMessageType,
                };

                // Get the IPEndPoint of the client to whom you want to send the acknowledgment
                if(clients.TryGetValue(playerID, out IPEndPoint clientEndPoint))
                {
                    // send the acknowledgment back to the client
                    await SendAsync((Acknowledgement)acknowledgmentMessage, MessageType.Acknowledgement, MessagePriority.low, clientEndPoint);
                }
            }            
        }

        #region ACKTry
        // TODO: Tracking of ack

        // Structure to hold message information
        public struct UnacknowledgedMessageInfo
        {
            public NetworkMessage Message;  // The actual message object
            public MessageType Type;        // Type of the message
            public MessagePriority Priority; // Priority level
            public IPEndPoint ClientEP;     // Client's IP address and port
            public DateTime Timestamp;      // Time when the message was sent
            public int Attempts;            // Number of resend attempts

            public UnacknowledgedMessageInfo(NetworkMessage message, MessageType type, MessagePriority priority, IPEndPoint clientEP, DateTime timestamp)
            {
                Message = message;
                Type = type;
                Priority = priority;
                ClientEP = clientEP;
                Timestamp = timestamp;
                Attempts = 0;  
            }
        }


        
        // Method to add a message to the unacknowledged list
        public static void TrackMessage(NetworkMessage message, MessageType type, MessagePriority priority, IPEndPoint clientEP, byte clientId)
        {
            //Guid messageId = Guid.NewGuid();  // Generate a unique ID for this message
            UnacknowledgedMessageInfo messageInfo = new UnacknowledgedMessageInfo(message, type, priority, clientEP, DateTime.UtcNow);
            unacknowledgedMessages.TryAdd(message.MessageId, messageInfo);


        }

        // Method to remove a message from the unacknowledged list 
        public static void AcknowledgeMessage(Guid messageId)
        {
            unacknowledgedMessages.TryRemove(messageId, out _);
        }

        private static async Task RetryUnacknowledgedMessages()
        {
            foreach(var entry in unacknowledgedMessages)
            {
                var messageId = entry.Key;
                var messageInfo = entry.Value;

                if(messageInfo.Attempts >= 3)
                {
                    // Remove if max attempts reached
                    unacknowledgedMessages.TryRemove(messageId, out _);
                    continue;
                }

                // Resend the message
                switch (messageInfo.Type)
                {
                    case MessageType.ClientHasJoined:
                        await SendAsync((ClientHasJoined)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ClientJoinAnswer:
                        await SendAsync((ClientJoinAnswer)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ClientHasLeft:
                        await SendAsync((ClientHasLeft)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ServerInfoMessage:
                        await SendAsync((ServerInfoMessage)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.PlayerJoined:
                        await SendAsync((PlayerJoined)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.PlayerLeft:
                        await SendAsync((PlayerLeft)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.PlayerUpdate:
                        await SendAsync((PlayerUpdate)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.PlayerSnapShot:
                        await SendAsync((PlayerSnapShot)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.PlayerInfoUpdate:
                        await SendAsync((PlayerInfoUpdate)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.LaserShot:
                        await SendAsync((LaserShot)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ChatMessage:
                        await SendAsync((ChatMessage)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ChatCommand:
                        await SendAsync((ChatCommand)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.ChatAcknowledgement:
                        await SendAsync((ChatAcknowledgement)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.Error:
                        //await SendAsync((Eror)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.Acknowledgement:
                        await SendAsync((Acknowledgement)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                    case MessageType.res4:
                        //await SendAsync((Res4)messageInfo.Message, messageInfo.Type, messageInfo.Priority, messageInfo.ClientEP);
                        break;
                }

                // Increment the attempt count
                messageInfo.Attempts++;
                unacknowledgedMessages[messageId] = messageInfo;
            }
        }

        public static void StartRetryMechanism()
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    await RetryUnacknowledgedMessages();
                    await Task.Delay(5000);  // Wait for 5 seconds before the next retry scan
                }
            });
        }


        // Felt til at stoppe timer-loopet
        private static bool stopTimer = false;
        private const int SnapShotSpeed = 30;

        private static async Task ContinouslySendPlayerSnapShot()
        {
            // Reset stop flag
            stopTimer = false;

            // Frekvens for snapshots
            int interval = (int)(1000f / SnapShotSpeed); // snapshotSpeed er antallet af snapshots pr. sekund 1000/30=33,3pr sek

            while (!stopTimer)
            {
                // Henter alle klienter fra ClientManager
                ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();

                // Tjek om der er nogen klienter
                if (clients.Count == 0)
                {
                    await Task.Delay(interval);
                    continue;
                }

                // Hent den nyeste snapshot
                PlayerSnapShot[] playerSnapShots = snapshotManager.GetLatestWorldStateSnapshot();
                if (playerSnapShots == null)
                {
                    await Task.Delay(interval);
                    continue;
                }

                // Send snapshot til alle klienter
                foreach (PlayerSnapShot pSnapshot in playerSnapShots)
                {
                    await SendDataToClients(pSnapshot, MessageType.PlayerSnapShot, MessagePriority.Low);
                }

                await Task.Delay(interval);
            }
        }

        // Metode til at stoppe timeren
        public static void StopTimer()
        {
            stopTimer = true;
        }
        #endregion
    }
}
