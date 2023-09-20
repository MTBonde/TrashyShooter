using System.Net;

using MessagePack;

namespace SharedData
{
    public static class MessageData
    {
        public enum MessageType : byte
        {
            // Connection
            Join = 0,  // 0000
            JoinAnswer = 1,  // 0001
            Leave = 2,  // 0010
            PlayerJoined = 3,  // 0011
            PlayerLeft = 4,  // 0100

            // Player
            PlayerUpdate = 5,  // 0101
            PlayerSnapShot = 6,  // 0110
            PlayerInfoUpdate = 7,  // 0111 
            
            // Spiltilstand
            Heartbeat = 8,  // 1000 
            GameState = 9,  // 1001

            // Spillerhandlinger
            PlayerAction = 10,  // 1010

            // Events og kontrol
            GameEvent = 11,  // 1011

            // Kommunikation
            Chat = 12,  // 1100

            // Fejl
            Error = 13,  // 1101 

            // Rserve
            res1 = 14,
            res2 = 15,
            res3 = 16
        }



        public enum MessagePriority : byte
        {
            Low = 0,
            High = 1
        }

        // Encoding the header byte
        public static byte EncodeHeader(MessageType messageType, MessagePriority priority, byte extraBits)
        {
            return (byte)((byte)priority << 7 | (byte)messageType << 3 | (extraBits & 0b00000111));
        }

        // Decoding the header byte
        public static (MessageType, MessagePriority, byte) DecodeHeader(byte header)
        {
            MessagePriority priority = (MessagePriority)(header >> 7);
            MessageType messageType = (MessageType)((header >> 3) & 0b00001111);
            byte extraBits = (byte)(header & 0b00000111);
            return (messageType, priority, extraBits);
        }


        // Serialize the message object to a byte array with MessagePack
        public static byte[] SerializeMessage<T>(T message)
        {
            return MessagePackSerializer.Serialize(message);
        }

        // Deserialize the byte array back to a message object with MessagePack
        public static T DeserializeMessage<T>(byte[] messageBytes)
        {
            return MessagePackSerializer.Deserialize<T>(messageBytes);
        }

        public static void SendNetworkMessage(object message, MessageType messageType, MessagePriority priority, IPEndPoint clientEP)
        {
            // Step 1: Encode the header
            byte header = EncodeHeader(messageType, priority, 0); // extraBits set to 0 for now, but what to do with them

            // Step 2: Serialize the message object  with MessagePack
            byte[] messageBytes = MessagePackSerializer.Serialize(message);

            // Step 3: Combine header and message
            byte[] combinedBytes = new byte[1 + messageBytes.Length];
            combinedBytes[0] = header;
            Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);

            // Step 4: Send via UDP, we will leave this line in server class
            //udpServer.Send(combinedBytes, combinedBytes.Length, clientEP); 
        }

        public static (object Message, MessageType Type, MessagePriority Priority) ReceiveNetworkMessage(byte[] receivedBytes)
        {
            // Step 1: Decode the header
            var (decodedMessageType, decodedPriority, _) = DecodeHeader(receivedBytes[0]);

            // Step 2: Extract the message payload
            byte[] messageBytes = new byte[receivedBytes.Length - 1];
            Buffer.BlockCopy(receivedBytes, 1, messageBytes, 0, messageBytes.Length);

            // Step 3: Deserialize the message object  with MessagePack
            object message = MessagePackSerializer.Deserialize<object>(messageBytes);

            return (message, decodedMessageType, decodedPriority);
        }


    }
}
