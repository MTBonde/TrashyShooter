using SharedData;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GameServer
{
    // Main server class 
    public class UDPGameServer
    {
        // UDP client to manage incoming and outgoing data.
        public UdpClient udpServer;
        // IPEndPoint to store client info.
        private IPEndPoint endPoint;

        private ChatManager chatManager;
        private ClientManager clientManager = new();
        private MessageHandler messageHandler;
        private SnapshotManager snapshotManager = new SnapshotManager();
        private GameWorldManager worldManager;
        private PlayerManager playerManager = new PlayerManager();
        private LagCompensationManager lagCompensationManager;
        private GameLogicController controller;

        //private ConcurrentDictionary<byte, IPEndPoint> clients = new ConcurrentDictionary<byte, IPEndPoint>();
        private byte nextAvailableID = 0;
        // Låseobjekt til at sikre, at ID-generering er atomar.
        private readonly object lockObject = new object();
        private Queue<byte> reusableIDs = new Queue<byte>();

        // Constructor: Initialize the server.
        public UDPGameServer(int port)
        {
            // Initialize UdpClient and bind it to the given port.
            udpServer = new UdpClient(port);
            // Initialize IPEndPoint to capture client data.
            endPoint = new IPEndPoint(IPAddress.Any, port);

            MessageSender.Initialize(udpServer, clientManager, snapshotManager);

            lagCompensationManager = new LagCompensationManager(snapshotManager);

            controller = new GameLogicController(snapshotManager, lagCompensationManager, playerManager);

            messageHandler = new MessageHandler(controller, snapshotManager, clientManager.clients);
            chatManager = new ChatManager(clientManager, playerManager);

            playerManager.OnPlayerStatChanged += UpdatePlayerInfo;
        }

        //TODO: flyt et bedre sted hen
        void UpdatePlayerInfo(NetworkMessage updateMessage, byte id)
        {
            MessageSender.SendAsync((PlayerInfoUpdate)updateMessage, MessageType.PlayerInfoUpdate, MessagePriority.Low, clientManager.clients[id]);
        }

        /// <summary>
        /// Asynchronous method for starting the server
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            IPEndPoint RemoteEndPoint = null;  
            Console.WriteLine("Server startet.");
            while(true)
            {
                try
                {
                    // Forsøger at modtage en besked fra en UDP-server.
                    var result = await udpServer.ReceiveAsync();

                    // Bestemmer spillerens ID baseret på den modtagne besked.
                    byte playerID = clientManager.DeterminePlayerID(result.RemoteEndPoint); 
                    

                    // Håndterer den modtagne besked ved hjælp af MessageHandler eller chat.
                    await messageHandler.HandleIncomingMessage(result.Buffer, playerID);

                    await chatManager.HandleIncomingMessage(result.Buffer, playerID);

                }
                catch(Exception e)
                {
                    // Generel fejlhåndtering.
                    // finder typen af den fangede exception.
                    string exceptionType = e.GetType().ToString();

                    // Udskriver beskeden og typen af exception.
                    Console.WriteLine($"!ERROR! An unexpected error occurred: {e.Message}. Exception Type: {exceptionType}");
                    Console.WriteLine($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
                    // Guard clause for non-SocketException types
                    if (!(e is SocketException))
                    {
                        return;
                    }

                    // Log SocketException
                    Console.WriteLine($"Socket Exception: {e.Message}");

                    // Guard clause for null capturedEndPoint
                    if(RemoteEndPoint == null)
                    {
                        return;
                    }

                    // Håndter den fangede socketexception
                    clientManager.HandleClientLeftUnexpectedly((SocketException)e, RemoteEndPoint);
                }                
            }
        }
    }
}
