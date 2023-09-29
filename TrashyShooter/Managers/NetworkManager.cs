using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using MessagePack;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using SharedData;
using REST_API;
using System.Text.Json;

namespace MultiplayerEngine
{
    public static class NetworkManager
    {

        #region Multiplayer

        public static UDPGameClient GameClient;
        public static event Action<NetworkMessage> OnMessageReceived;

        public class UDPGameClient
        {
            UdpClient udpClient;
            IPEndPoint endPoint;
            Action<byte[]> OnDataRecievedEvent;
            public readonly float ServerUpdateRate = 30;

            public UDPGameClient()
            {
                udpClient = new UdpClient();
            }

            public void JoinServer(string ipAddress = null)
            {
                IPAddress serverIP;
                if(ipAddress == "HJEMME")
                    serverIP = IPAddress.Parse("192.169.39.190");//hjemme
                else if (ipAddress != null && ipAddress != "")//TODO: make It check if it is a real ip address
                    serverIP = IPAddress.Parse(ipAddress);//customIP
                else
                    serverIP = IPAddress.Parse("127.0.0.1");//local
                //IPAddress serverIP = IPAddress.Parse("10.131.66.195");//skole
                //IPAddress serverIP = IPAddress.Parse("192.169.39.1");//GD
                int serverPort = 8080;
                endPoint = new IPEndPoint(serverIP, serverPort);
                udpClient.Connect(endPoint);
                Thread recieveTrhread = new Thread(() => RecieveDataFromServer());
                recieveTrhread.IsBackground = true;
                recieveTrhread.Start();
                this.OnDataRecievedEvent = OnDataRecieved;
            }

            readonly int ackAmount = 5;
            private void OnDataRecieved(byte[] receivedData)
            {
                (NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);
                NetworkMessage message = messageInfo.Message;
                if (messageInfo.Priority == MessagePriority.High)
                {
                    Acknowledgement acknowledgement = new Acknowledgement();
                    acknowledgement.MessageId = message.MessageId;
                    acknowledgement.OriginalMessageType = messageInfo.Type;
                    acknowledgement.PriorityMessage = false;
                    for (int i = 0; i < ackAmount; i++)
                    {
                        GameWorld.Instance.gameClient.SendDataToServer(acknowledgement);
                    }
                }
                switch (messageInfo.Type)
                {
                    case MessageType.ClientJoinAnswer:
                        ClientJoinAnswer joinAnswer = (ClientJoinAnswer)message;
                        GameObject yourPlayer = new GameObject();
                        yourPlayer.AddComponent<Player>().Setup(true);
                        yourPlayer.GetComponent<Sender>().SetID(joinAnswer.playerID);
                        return;
                    case MessageType.PlayerJoined:
                        GameObject joinedPlayer = new GameObject();
                        joinedPlayer.AddComponent<Player>().Setup(false);
                        joinedPlayer.GetComponent<Receiver>().SetID(((PlayerJoined)message).playerID);
                        return;
                    case MessageType.ClientHasLeft:
                        PlayerLeft leftData = (PlayerLeft)message;
                        foreach (GameObject go in SceneManager.active_scene.gameObjects)
                        {
                            if (go.Components.OfType<Receiver>().Any())
                                if (go.GetComponent<Receiver>().GetId() == leftData.playerID)
                                {
                                    SceneManager.active_scene.gameObjects.Remove(go);
                                    Debug.WriteLine("Removed Player succesful: " + go);
                                    break;
                                }
                        }
                        Debug.WriteLine("Did't find player");
                        return;
                    case MessageType.PlayerSnapShot:
                        message = (PlayerSnapShot)messageInfo.Message;
                        break;
                    case MessageType.PlayerInfoUpdate:
                        message = (PlayerInfoUpdate)messageInfo.Message;
                        break;
                    case MessageType.LaserShot:
                        message = (LaserShot)messageInfo.Message;
                        break;
                    case MessageType.ChatMessage:
                        message = (ChatMessage)messageInfo.Message; 
                        break;
                    case MessageType.ServerInfoMessage:
                        message = (ServerInfoMessage)messageInfo.Message;
                        break;
                    case MessageType.Acknowledgement:
                        AckReceived(((Acknowledgement)message).MessageId);
                        break;
                    //case MessageType.ScoreboardUpdate:
                    //    message = (JoinAnswer)messageInfo.Message;
                    //    break;
                    default:
                        return;
                }
                OnMessageReceived?.Invoke(message);
            }

            ///only reason for async is to support the fake latency
            public async void SendDataToServer<T>(T message) where T : NetworkMessage
            {
                //await Task.Delay(2000);
                MessagePriority priority = MessagePriority.Low;
                if(message.PriorityMessage == true)
                {
                    priority = MessagePriority.High;
                    Guid id = new Guid();
                    message.MessageId = id;
                    AddAckMessage(message, id);
                }
                udpClient.Send(NetworkMessageProtocol.SendNetworkMessage<T>(message, message.MessageType, priority, endPoint).MessageBytes);
                //byte[] messageBytes = new byte[1024];
                //byte messageTypeByte = message.GetMessageTypeAsByte;
                //switch (message.MessageType)
                //{
                //    //We dont wont to send snapshots, only recive :)
                //    case MessageType.PlayerUpdate:
                //        messageBytes = MessagePackSerializer.Serialize((PlayerUpdate)message);
                //        break;
                //    case MessageType.PlayerLeft:
                //        messageBytes = MessagePackSerializer.Serialize((Leave)message);
                //        break;
                //    case MessageType.ClientJoinAnswer:
                //        messageBytes = MessagePackSerializer.Serialize((Join)message);
                //        break;
                //    default:
                //        break;
                //}
                //byte[] combinedBytes = new byte[1 + messageBytes.Length];
                //combinedBytes[0] = messageTypeByte;
                //Buffer.BlockCopy(messageBytes, 0, combinedBytes, 1, messageBytes.Length);
                //udpClient.Send(combinedBytes);
            }
            void RecieveDataFromServer()
            {
                while (true)
                {
                    byte[] serverResponse = udpClient.Receive(ref endPoint);
                    OnDataRecievedEvent.Invoke(serverResponse);
                }
            }

            public class MessageInfo<T> where T : NetworkMessage
            {

                public T message;
                public Guid messageID;

                public MessageInfo(T message, Guid messageID)
                {
                    this.message = message;
                    this.messageID = messageID;
                }
            }

            Dictionary<Guid, bool> received = new Dictionary<Guid, bool>();

            public void AddAckMessage<T>(T message, Guid id) where T : NetworkMessage
            {
                MessageInfo<T> messageInfo = new MessageInfo<T>(message, id);
                received.Add(messageInfo.messageID, false);
                RetrySendMessage(messageInfo);
            }

            public void AckReceived(Guid id)
            {
                received[id] = true;
            }

            public async Task RetrySendMessage<T>(MessageInfo<T> messageToAck) where T : NetworkMessage
            {
                while (!received[messageToAck.messageID])
                {
                    await Task.Delay(1000);
                    if (received[messageToAck.messageID])
                        break;
                    SendDataToServer(messageToAck.message);
                }
                received.Remove(messageToAck.messageID);
            }
        }
        #endregion

        #region Rest
        public static HttpClient restClient = new HttpClient();

        public static void SetupRest(string address)
        {
            if (address == "")
            {
                restClient = new HttpClient
                {
                    BaseAddress = new Uri("https://localhost:7159/api/Scoreboard/"),
                    Timeout = TimeSpan.FromSeconds(10)
                };
            }
            else
            {
                restClient = new HttpClient
                {
                    BaseAddress = new Uri($"https://{address}:7159/api/Scoreboard/"),
                    Timeout = TimeSpan.FromSeconds(10)
                };
            }
        }

        public static HttpClient GetRestClient()
        {
            return restClient;
        }

        public static async Task<List<ScoreboardModel>> GetScoreboard()
        {
            try
            {
                HttpResponseMessage response = await GetRestClient().GetAsync("GetScoreboard");
                if (response.IsSuccessStatusCode)
                {
                    string jsonContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(jsonContent);
                    var scoreboard = JsonSerializer.Deserialize<List<ScoreboardModel>>(jsonContent);
                    return scoreboard;
                }
                else 
                { 
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }
        #endregion

        #region Database
        public static readonly HttpClient databaseClient = new HttpClient();

        public static async void AddTimeScore(string name, string score, TextRenderer scoreBoardText)
        {
            var values = new Dictionary<string, string>
              {
                  { "scoreName", name },
                  { "scoreScore", score }
              };

            var content = new FormUrlEncodedContent(values);

            Console.WriteLine("web");
            try
            {
                using HttpResponseMessage response = await databaseClient.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/AddTimeScore.php", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                GetTimeScores(scoreBoardText);
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static async void GetTimeScores(TextRenderer textRend)
        {
            var values = new Dictionary<string, string>
              {
              };

            var content = new FormUrlEncodedContent(values);

            Console.WriteLine("web");
            try
            {
                using HttpResponseMessage response = await databaseClient.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/GetTimeScores.php", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                if(textRend != null)
                    textRend.SetText(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static async void AddFloorScore(string name, string score, string time, TextRenderer scoreBoardText)
        {
            var values = new Dictionary<string, string>
              {
                  { "scoreName", name },
                  { "scoreScore", score },
                  { "scoreTime", time }
              };

            var content = new FormUrlEncodedContent(values);

            Console.WriteLine("web");
            try
            {
                using HttpResponseMessage response = await databaseClient.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/AddFloorScore.php", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                GetFloorScores(scoreBoardText);
                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public static async void GetFloorScores(TextRenderer textRend)
        {
            var values = new Dictionary<string, string>
            {
            };

            var content = new FormUrlEncodedContent(values);

            Console.WriteLine("web");
            try
            {
                using HttpResponseMessage response = await databaseClient.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/GetFloorScores.php", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                if (textRend != null)
                    textRend.SetText(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        #endregion
    }
}