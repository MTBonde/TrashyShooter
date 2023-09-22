using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using SharedData;

namespace GameServer
{
    public class MessageSender
    {
        private UdpClient udpServer;
        private ClientManager clientManager;  

        public MessageSender(UdpClient udpServer, ClientManager clientManager)
        {
            this.udpServer = udpServer;
            this.clientManager = clientManager;
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

        // Send to all clients
        public async Task SendDataToClients(object message, MessageType messageType, MessagePriority priority)
        {
            // Use the ClientManager to get the client list
            var clients = clientManager.GetClients();  
            foreach(var client in clients)
            {
                await SendAsync(message, messageType, priority, client.Value);
            }
        }

        public async Task SendDataToClientsExceptOne(object message, MessageType messageType, MessagePriority priority, byte exceptionID)
        {
            // Use the ClientManager to get the client list
            var clients = clientManager.GetClients(); 
            foreach(var client in clients)
            {
                if(client.Key != exceptionID)
                {
                    await SendAsync(message, messageType, priority, client.Value);
                }
            }
        }
    }
}
