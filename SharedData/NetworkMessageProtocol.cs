using System.Net;

using MessagePack;

namespace SharedData
{
    /// <summary>
    /// Static Helper Methods for Encoding, Decoding Serialization and De-serialization.
    /// </summary>
    public class NetworkMessageProtocol
    {
        /// <summary>
        /// Encodes the message header by combining the message type, priority, and any extra bits into a single byte.
        /// </summary>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="priority">The priority level of the message.</param>
        /// <param name="extraBits">Any extra bits that need to be included in the header.</param>
        /// <returns>The encoded header as a byte.</returns>
        public static byte EncodeHeader(MessageType messageType, MessagePriority priority, byte extraBits)
        {
            // Denne linje koder en header-byte, der kombinerer beskedprioritet, beskedtype og eventuelle ekstra bits.
            // priority << 7: Forskyder priority til de mest betydende bits (MSB).
            // !!Der er kun én bit afsat til priority, så det vil enten være 0 eller 1.
            // messageType << 3: Forskyder messageType til de næste 4 bits. Da messageType er af typen byte, vil det fylde 4 bits.
            // extraBits & 0b00000111: Anvender en bitmaske til at sikre, at kun de sidste 3 bits af extraBits er brugt.
            // Til sidst kombineres alle tre ved hjælp af bitwise OR (|) for at danne en enkelt byte.
            return (byte)((byte)priority << 7 | (byte)messageType << 3 | (extraBits & 0b00000111));

        }

        /// <summary>
        /// Decodes the message header, extracting the message type, priority, and any extra bits from a single byte.
        /// </summary>
        /// <param name="header">The encoded header as a byte.</param>
        /// <returns>A tuple containing the decoded message type, priority, and extra bits.</returns>
        public static (MessageType, MessagePriority, byte) DecodeHeader(byte header)
        {
            // Forskyder header med 7 bits til højre for at isolere priority-bit'et.
            // Derefter castes det til MessagePriority-typen.
            MessagePriority priority = (MessagePriority)(header >> 7);

            // Forskyder header med 3 bits og anvender en bitmaske for at isolere de 4 bits, der repræsenterer messageType.
            // Derefter castes det til MessageType-typen.
            MessageType messageType = (MessageType)((header >> 3) & 0b00001111);

            // Anvender en bitmaske for at isolere de sidste 3 bits, der repræsenterer ekstra bits.
            byte extraBits = (byte)(header & 0b00000111);

            // Returnerer de afkodede værdier i en tuple.
            return (messageType, priority, extraBits);

        }

        /// <summary>
        /// Serializes the message object into a byte array using MessagePack.
        /// </summary>
        /// <typeparam name="T">The type of the message object.</typeparam>
        /// <param name="message">The message object to serialize.</param>
        /// <returns>The serialized byte array.</returns>
        public static byte[] SerializeMessage<T>(T message)
        {
            // Serialiserer beskedobjektet til en byte-array med MessagePack
            return MessagePackSerializer.Serialize(message);
        }

        /// <summary>
        /// Deserializes a byte array back into a message object using MessagePack.
        /// </summary>
        /// <typeparam name="T">The type of the message object.</typeparam>
        /// <param name="messageBytes">The byte array to deserialize.</param>
        /// <returns>The deserialized message object.</returns>
        public static T DeserializeMessage<T>(byte[] messageBytes)
        {
            // Dekoder byte-array tilbage til et beskedobjekt med MessagePack
            return MessagePackSerializer.Deserialize<T>(messageBytes);
        }


        /// <summary>
        /// Creates a network message by encoding the header and serializing the message object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="priority">Priority level of the message.</param>
        /// <param name="clientEP">Client endpoint.</param>
        /// <returns>A tuple containing the message bytes, the length of the message, and the client endpoint.</returns>
        public static (byte[] MessageBytes, int Length, IPEndPoint ClientEP) SendNetworkMessage<T> (T message,
                                                                                                MessageType messageType,
                                                                                                MessagePriority priority,
                                                                                                IPEndPoint clientEP) where T : NetworkMessage
        {
            try
            {
                // Trin 1: Koder headeren
                byte header = EncodeHeader(messageType, priority, 0);  // extraBits sat til 0 for nu, men hvad skal der ske med dem?

                // Trin 2: Serialiserer beskedobjektet med MessagePack
                byte[] messageBytes = MessagePackSerializer.Serialize((T)message);

                // Trin 3: Kombinerer header og besked
                byte[] combinedBytes = new byte[1 + messageBytes.Length];
                combinedBytes[0] = header;
                Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);

                // Returnerer en tuple med beskedbytes, længde og klientendpoint
                return (combinedBytes, combinedBytes.Length, clientEP);
            }
            catch (Exception ex)
            {
                // Log exception her
                Console.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return (null, 0, null);  // Returner en tom tuple som indikator for fejl
            }
        }

        /// <summary>
        /// Receives and decodes a network message that consists of a message, its type, and its priority.
        /// </summary>
        /// <param name="receivedBytes">The received byte array.</param>
        /// <returns>A tuple containing the decoded message, its type, and its priority.</returns>
        public static (NetworkMessage message, MessageType Type, MessagePriority Priority) ReceiveNetworkMessage(byte[] receivedBytes)
        {
            // Trin 1: Dekoder headeren
            // Headeren, som indeholder beskedtypen og prioritet, dekodes fra det første byte.
            var (decodedMessageType, decodedPriority, _) = DecodeHeader(receivedBytes[0]);

            // Trin 2: Uddrager beskedens payload
            // En ny byte-array oprettes til at holde selve beskeden, idet vi springer headeren over.
            byte[] messageBytes = new byte[receivedBytes.Length - 1];
            Buffer.BlockCopy(receivedBytes, 1, messageBytes, 0, messageBytes.Length);

            // Trin 3: Dekoder beskedobjektet med MessagePack
            // Beskeden dekodes fra byte-array til objekt.
            NetworkMessage message;

            switch(decodedMessageType)
            {
                // Forbindelse
                case MessageType.ClientHasJoined:
                    message = MessagePackSerializer.Deserialize<Join>(messageBytes);
                    break;
                case MessageType.ClientJoinAnswer:
                    message = MessagePackSerializer.Deserialize<JoinAnswer>(messageBytes);
                    break;
                case MessageType.ClientHasLeft:
                    message = MessagePackSerializer.Deserialize<Leave>(messageBytes);
                    break;
                //case MessageType.Heartbeat:
                //    message = MessagePackSerializer.Deserialize<Heartbeat>(messageBytes);
                //    break;

                // Spiller
                case MessageType.PlayerJoined:
                    message = MessagePackSerializer.Deserialize<PlayerJoined>(messageBytes);
                    break;
                case MessageType.PlayerLeft:
                    message = MessagePackSerializer.Deserialize<PlayerLeft>(messageBytes);
                    break;
                case MessageType.PlayerUpdate:
                    message = MessagePackSerializer.Deserialize<PlayerUpdate>(messageBytes);
                    break;
                case MessageType.PlayerSnapShot:
                    message = MessagePackSerializer.Deserialize<PlayerSnapShot>(messageBytes);
                    break;
                case MessageType.PlayerInfoUpdate:
                    message = MessagePackSerializer.Deserialize<PlayerInfoUpdate>(messageBytes);
                    break;

                // Visuel
                case MessageType.LaserShot:
                    message = MessagePackSerializer.Deserialize<LaserShot>(messageBytes);
                    break;

                // Kommunikation
                case MessageType.ChatMessage:
                    message = MessagePackSerializer.Deserialize<ChatMessage>(messageBytes);
                    break;
                case MessageType.ChatCommand:
                    message = MessagePackSerializer.Deserialize<ChatCommand>(messageBytes);
                    break;
                case MessageType.ChatAcknowledgement:
                    message = MessagePackSerializer.Deserialize<ChatAcknowledgement>(messageBytes);
                    break;

                // Fejl
                //case MessageType.Error:
                //    message = MessagePackSerializer.Deserialize<Error>(messageBytes);
                //    break;

                // Reserveret
                case MessageType.Acknowledgement:
                    message = MessagePackSerializer.Deserialize<Acknowledgement>(messageBytes);
                    break;
                //case MessageType.res4:
                //    message = MessagePackSerializer.Deserialize<res4>(messageBytes);
                //    break;

                // Hvis beskedtypen ikke findes
                default:
                    Console.WriteLine("BESKEDEN FINDES IKKE: " + decodedMessageType);
                    message = null;
                    break;
            }

            // Returnerer en tuple med den dekodede besked, dens type og prioritet
            return (message, decodedMessageType, decodedPriority);
        }
    }
}
