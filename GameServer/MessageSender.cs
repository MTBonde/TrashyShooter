using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using SharedData;

namespace GameServer
{
    public class MessageSender
    {
        private UdpClient udpServer;
        
        public MessageSender(UdpClient udpServer)
        {
            this.udpServer = udpServer;
        }

        /// <summary>
        /// Asynchronously sends a message to a client using the specified message type and priority.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <param name="messageType">The type of the message.</param>
        /// <param name="priority">The priority level of the message.</param>
        /// <param name="clientEP">The endpoint of the client to send the message to.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendAsync(object message, MessageType messageType, MessagePriority priority, IPEndPoint clientEP)
        {
            // Bruger NetworkMessageProtocol til at lave en samlet netværksbesked
            var networkMessage = NetworkMessageProtocol.SendNetworkMessage(message, messageType, priority, clientEP);

            // Sender den samlede netværksbesked via UDP
            await udpServer.SendAsync(networkMessage.MessageBytes, networkMessage.Length, networkMessage.ClientEP);
        }

    }
}
