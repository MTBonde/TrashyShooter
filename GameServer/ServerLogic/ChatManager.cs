using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedData;

namespace GameServer
{
    public class ChatManager
    {
        private ClientManager clientManager;
        private MessageSender messageSender;

        public ChatManager(ClientManager clientManager, MessageSender messageSender)
        {
            this.clientManager = clientManager;
            this.messageSender = messageSender;
        }

        // Handles incoming chat message
        public async Task HandleIncomingChatMessage(NetworkMessageChat messageChat, byte playerID)
        {
            // Broadcast the chat message to all connected clients
            await messageSender.SendDataToClients(messageChat, MessageType.ChatMessage, MessagePriority.Low);
        }

        // Handle other message types as needed
        // ...
    }
}
