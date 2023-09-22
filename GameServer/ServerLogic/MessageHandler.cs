﻿using System.Collections.Concurrent;
using System.Net;

using MessagePack;

using SharedData;

namespace GameServer
{
    public class MessageHandler
    {
        // Delegeret til håndtering af beskeder
        //public delegate Task MessageHandlerDelegate(byte[] dataToDeserialize, byte playerID);
        public delegate Task MessageHandlerDelegate((object Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID);


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
                { MessageType.Acknowledgement, HandleAcknowledgement },
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
            (object message, MessageType messageType, MessagePriority messagePriority) = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);

           

            if(messageHandlers.TryGetValue(messageType, out MessageHandlerDelegate handler))
            {
                Console.WriteLine($"Received message of type {messageType} from player {playerID}");

                // Process the message
                await handler((message, messageType, messagePriority), playerID);

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
        private async Task HandleClientHasJoined((object Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            Console.WriteLine($"Klient med ID {playerID} har tilsluttet sig.");

            Join join = new Join();

            // Delegate to GameController
            //TODO: GCL gameLogicController.HandleJoin(playerID, join.playerName);

            JoinAnswer answer = new JoinAnswer { playerID = playerID };
            PlayerJoined playerJoined = new PlayerJoined { playerID = playerID };



            // TODO: MESSAGSENDER await SendDataToClientAsync(answer, playerID);
            // TODO: MESSAGSENDER await SendDataToClientsExceptOne(playerJoined, playerID);
            // TODO: MESSAGSENDER await NotifyOtherClients(playerID, thisClientEndPoint);

            await Task.CompletedTask;
        }

        private async Task HandleClientJoinAnswer((object Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
        {
            // Her kan du indsætte den faktiske logik for at håndtere en "ClientJoinAnswer" besked
            Console.WriteLine($"Klient med ID {playerID} har modtaget et svar om deltagelse.");
            await Task.CompletedTask;
        }

        private async Task HandleAcknowledgement((object Message, MessageType Type, MessagePriority Priority) messageInfo, byte playerID)
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
