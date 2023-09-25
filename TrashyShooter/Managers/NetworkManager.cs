﻿using System.Net.Http;
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

            private void OnDataRecieved(byte[] receivedData)
            {
                (NetworkMessage Message, MessageType Type, MessagePriority Priority) messageInfo = NetworkMessageProtocol.ReceiveNetworkMessage(receivedData);
                NetworkMessage message;
                switch (messageInfo.Type)
                {
                    case MessageType.ClientJoinAnswer:
                        JoinAnswer joinAnswer = (JoinAnswer)messageInfo.Message;
                        GameObject yourPlayer = new GameObject();
                        yourPlayer.AddComponent<Player>().Setup(true);
                        yourPlayer.GetComponent<Sender>().SetID(joinAnswer.playerID);
                        return;
                    case MessageType.PlayerJoined:
                        message = (PlayerJoined)messageInfo.Message;
                        GameObject joinedPlayer = new GameObject();
                        joinedPlayer.AddComponent<Player>().Setup(false);
                        joinedPlayer.GetComponent<Receiver>().SetID(((PlayerJoined)message).playerID);
                        return;
                    case MessageType.ClientHasLeft:
                        PlayerLeft leftData = (PlayerLeft)messageInfo.Message;
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
                        message = (PlayerUpdate)messageInfo.Message;
                        break;
                    case MessageType.LaserShot:
                        message = (LaserShot)messageInfo.Message;
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
            public async void SendDataToServer(NetworkMessage message)
            {
                //await Task.Delay(2000);
                udpClient.Send(NetworkMessageProtocol.SendNetworkMessage(message, message.MessageType, MessagePriority.Low, endPoint).MessageBytes);
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
        }
        #endregion

        #region Database
        public static readonly HttpClient client = new HttpClient();

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
                using HttpResponseMessage response = await client.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/AddTimeScore.php", content);
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
                using HttpResponseMessage response = await client.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/GetTimeScores.php", content);
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
                using HttpResponseMessage response = await client.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/AddFloorScore.php", content);
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
                using HttpResponseMessage response = await client.PostAsync("https://dreamlikestudios.net/GameBackend/HorrorMaze/GetFloorScores.php", content);
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