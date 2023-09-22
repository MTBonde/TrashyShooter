﻿using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

using SharedData;

namespace GameServer
{
    public static class MessageSender
    {
        private static UdpClient udpServer;
        private static ClientManager clientManager;
        private static object lockObject = new object();

        public static void Initialize(UdpClient udpServer, ClientManager clientManager)
        {
            lock(lockObject)
            {
                if(MessageSender.udpServer == null && MessageSender.clientManager == null)
                {
                    MessageSender.udpServer = udpServer;
                    MessageSender.clientManager = clientManager;
                }
            }
        }

        /// <summary>
        /// Asynchronously sends a message to a client using the specified message type and priority.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="priority">The priority level of the message.</param>
        /// <param name="clientEP">The endpoint of the client to send the message to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task SendAsync(object message, MessageType messageType, MessagePriority priority, IPEndPoint clientEP)
        {
            // Bruger NetworkMessageProtocol til at lave en samlet netværksbesked
            (byte[] MessageBytes, int Length, IPEndPoint ClientEP) networkMessage = NetworkMessageProtocol.SendNetworkMessage(message, messageType, priority, clientEP);

            // Sender den samlede netværksbesked via UDP
            await udpServer.SendAsync(networkMessage.MessageBytes, networkMessage.Length, networkMessage.ClientEP);
        }

        // Send to all clients
        public static async Task SendDataToClients(object message, MessageType messageType, MessagePriority priority)
        {
            // Use the ClientManager to get the client list
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
            foreach(KeyValuePair<byte, IPEndPoint> client in clients)
            {
                await SendAsync(message, messageType, priority, client.Value);
            }
        }

        public static async Task SendDataToClientsExceptOne(object message, MessageType messageType, MessagePriority priority, byte exceptionID)
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

        public static async Task SendAcknowledgment(byte playerID, MessageType originalMessageType, Guid messageId)
        {
            ConcurrentDictionary<byte, IPEndPoint> clients = clientManager.GetClients();
            // Declare as object to handle different acknowledgment types
            object acknowledgmentMessage;
            if(originalMessageType == MessageType.ChatMessage || originalMessageType == MessageType.ChatCommand)
            {
                // Create a chat-specific acknowledgment message
                acknowledgmentMessage = new ChatAcknowledgement
                {
                    playerID = playerID,
                    OriginalMessageType = originalMessageType,
                    // Include messageId if you add it to the class definition
                };
            }
            else
            {
                // Create a generic acknowledgment message
                acknowledgmentMessage = new Acknowledgement
                {
                    playerID = playerID,
                    OriginalMessageType = originalMessageType,
                    // Include messageId if you add it to the class definition
                };
            }

            // Get the IPEndPoint of the client to whom you want to send the acknowledgment
            if(clients.TryGetValue(playerID, out IPEndPoint clientEndPoint))
            {
                // send the acknowledgment back to the client
                await SendAsync(acknowledgmentMessage, MessageType.Acknowledgement, MessagePriority.High, clientEndPoint);
            }
        }




        // TODO: Tracking of ack

        // Structure to hold message information
        public struct UnacknowledgedMessageInfo
        {
            public object Message;
            public MessageType Type;
            public MessagePriority Priority;
            public IPEndPoint ClientEP;
            public DateTime Timestamp;
            public byte ClientID;  

            public UnacknowledgedMessageInfo(object message, MessageType type, MessagePriority priority, IPEndPoint clientEP, DateTime timestamp, byte clientId)
            {
                Message = message;
                Type = type;
                Priority = priority;
                ClientEP = clientEP;
                Timestamp = timestamp;
                ClientID = clientId;  
            }
        }

        // Dictionary to hold unacknowledged messages
        public static ConcurrentDictionary<Guid, UnacknowledgedMessageInfo> unacknowledgedMessages = new ConcurrentDictionary<Guid, UnacknowledgedMessageInfo>();

        // Method to add a message to the unacknowledged list
        public static void TrackMessage(object message, MessageType type, MessagePriority priority, IPEndPoint clientEP, byte clientId)
        {
            Guid messageId = Guid.NewGuid();  // Generate a unique ID for this message
            UnacknowledgedMessageInfo messageInfo = new UnacknowledgedMessageInfo(message, type, priority, clientEP, DateTime.UtcNow, clientId);
            unacknowledgedMessages.TryAdd(messageId, messageInfo);


        }

        // Method to remove a message from the unacknowledged list (usually called when an acknowledgment is received)
        public static void AcknowledgeMessage(Guid messageId)
        {
            unacknowledgedMessages.TryRemove(messageId, out _);
        }

    }
}
