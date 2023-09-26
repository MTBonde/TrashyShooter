using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerEngine
{
    internal class Sender : Component
    {

        byte id;
        public Action<NetworkMessage> snapShotAction;
        public Action<NetworkMessage> LaserAction;
        public Action<NetworkMessage> ScoreUpdate;
        public Action<NetworkMessage> HudUpdate;
        public Action<NetworkMessage> ChatUpdate;

        public void SetID(byte id)
        {
            this.id = id;
            NetworkManager.OnMessageReceived += ReceiveData;
        }

        public void ReceiveData(NetworkMessage message)
        {
            switch (message.MessageType)
            {
                case MessageType.PlayerSnapShot:
                    PlayerSnapShot playerData = (PlayerSnapShot)message;
                    if (playerData.playerID == id)
                    {
                        snapShotAction?.Invoke(playerData);
                    }
                    break;
                case MessageType.LaserShot:
                    LaserShot laserShot = (LaserShot)message;
                    LaserAction?.Invoke(laserShot);
                    break;
                //case MessageType.ScoreboardUpdate:
                //    ScoreUpdate?.Invoke((ScoreboardUpdate)message);
                //    break;
                case MessageType.PlayerInfoUpdate:
                    HudUpdate?.Invoke((PlayerInfoUpdate)message);
                    break;
                case MessageType.ChatMessage:
                    ChatUpdate?.Invoke((ChatMessage)message);
                    break;
            }
        }

        public void SendData<T>(T message) where T : NetworkMessage
        {
            GameWorld.Instance.gameClient.SendDataToServer(message);
        }
    }
}
