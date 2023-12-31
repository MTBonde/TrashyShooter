﻿using System.Collections.Concurrent;
using System.Net;

using SharedData;

namespace GameServer
{
    public class MessageHandler
    {
        /// <summary>
        /// Delegate til håndtering af beskeder
        /// </summary>
        /// <param name="messageInfo"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public delegate Task MessageHandlerDelegate((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID);

        /// <summary>
        /// Dictionary til at mappe MessageType til den tilsvarende beskedhåndterer
        /// </summary>
        private Dictionary<MessageType, MessageHandlerDelegate> messageHandlers;

        private ConcurrentDictionary<byte, IPEndPoint> clients;

        private readonly GameLogicController gameLogicController;
        private readonly SnapshotManager snapshotManager;

        public MessageHandler(GameLogicController gameLogicController, SnapshotManager snapshotManager, ConcurrentDictionary<byte, IPEndPoint> clients)
        {
            this.gameLogicController = gameLogicController;
            this.snapshotManager = snapshotManager;
            this.clients = clients;
            // Initialiser messageHandlers ordbogen
            messageHandlers = new Dictionary<MessageType, MessageHandlerDelegate>
            {
                { MessageType.ClientHasJoined, HandleClientHasJoined },
                { MessageType.ClientHasLeft, HandleClientHasLeft },
               // { MessageType.Heartbeat, HandleHeartbeat },
              //  { MessageType.PlayerJoined, HandlePlayerJoined },
                { MessageType.PlayerLeft, HandlePlayerLeft },
                { MessageType.PlayerUpdate, HandlePlayerUpdate },        
                //{ MessageType.Error, HandleError },
                { MessageType.Acknowledgement, HandleAcknowledgement },
                //{ MessageType.res4, Handleres4 }
            };
        }

        /// <summary>
        /// Asynkron håndtering af indkomne beskeder, sorter dem og send dem videre til den korrekte handler alt efter beskedtype.
        /// </summary>
        /// <param name="receivedData">The modtagne Byte Array med beskeden</param>
        /// <param name="playerID">ID på afsender.</param>
        /// <returns></returns>

        public async Task HandleIncomingMessage(byte[] receivedData, byte playerID)
        {
            (NetworkMessage message, MessageType messageType, MessagePriority messagePriority) = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);



            if(messageHandlers.TryGetValue(messageType, out MessageHandlerDelegate handler))
            {
                if (messageType != MessageType.PlayerUpdate) Console.WriteLine($"Received message of type {messageType} from player {playerID}");

                // Process the message
                handler((message, messageType, messagePriority), playerID);

                // Send acknowledgment if the message is high priority
                if(messagePriority == MessagePriority.High)
                {
                    // TODO: ACK ID await MessageSender.SendAcknowledgment(playerID, messageType, messageId);
                }
            }
            else
            {
                Console.WriteLine($"Unknown message type: {messageType}");
            }
        }




        // beskedhåndteringsmetoder
        private async Task HandleClientHasJoined((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            Console.WriteLine($"Klient med ID {playerID} har tilsluttet sig.");

            // Opretter en ny Join-objekt for at håndtere spillerens indtræden i spillet
            ClientHasJoined clientHasJoined = (ClientHasJoined)messageInfo.Message;

            // delegeret til GameController
            gameLogicController.HandleJoin(playerID, clientHasJoined.playerName);

            // Opretter et svar og en besked om, at en ny spiller er kommet ind i spillet
            ClientJoinAnswer answer = new ClientJoinAnswer { playerID = playerID };
            PlayerJoined playerJoined = new PlayerJoined { playerID = playerID };

            // Finder klientens IPEndPoint fra ConcurrentDictionary
            if(clients.TryGetValue(playerID, out IPEndPoint thisClientEndPoint))
            {
                // Sender svar til klienten
                await MessageSender.SendAsync(answer,
                                              MessageType.ClientJoinAnswer,
                                              MessagePriority.High,
                                              thisClientEndPoint);
            }

            await MessageSender.SendDataToClientsExceptOne(playerJoined, MessageType.PlayerJoined, MessagePriority.Low, playerID);
            await MessageSender.NotifyOtherClients(playerID, thisClientEndPoint);

            await Task.CompletedTask;
        }

        private async Task HandleClientHasLeft((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Deserialiser dataene til en PlayerLeft-objekt
            ClientHasLeft clientHasLeft = (ClientHasLeft)messageInfo.Message;

            // Delegate to GLC
            gameLogicController.HandlePlayerLeft(playerID);

            // Sender PlayerLeft-beskeden til alle klienter
            await MessageSender.SendDataToClients(clientHasLeft, MessageType.ClientHasLeft, MessagePriority.Low);
        }

        public async Task HandlePlayerLeft((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Deserialiser dataene til en PlayerLeft-objekt
            PlayerLeft playerleft = (PlayerLeft)messageInfo.Message;

            // Delegate to GLC
            gameLogicController.HandlePlayerLeft(playerID);

            // Sender PlayerLeft-beskeden til alle klienter
            await MessageSender.SendDataToClients(playerleft, MessageType.PlayerLeft, MessagePriority.Low);
        }

        private async Task HandlePlayerUpdate((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            PlayerUpdate update = (PlayerUpdate)messageInfo.Message;

            // Delegate to GLC
            await gameLogicController.HandlePlayerUpdate(update, playerID);
        }






        private async Task HandleAcknowledgement((NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Try to cast the incoming message to Acknowledgement. If it fails, ack will be null.
            Acknowledgement ack = messageInfo.Message as Acknowledgement;

            // Check if the cast was successful
            if(ack != null)
            {
                // Log the received acknowledgment
                Console.WriteLine($"Received acknowledgment from player {playerID} for original message type {ack.OriginalMessageType}.");

                // Remove the message from the list of unacknowledged messages
                MessageSender.AcknowledgeMessage(ack.MessageId);
            }

            await Task.CompletedTask;
        }
    }
}
