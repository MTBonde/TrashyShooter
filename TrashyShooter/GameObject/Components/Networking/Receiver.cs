using SharedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiplayerEngine
{
    public class Receiver : Component
    {

        byte id;
        public Action<NetworkMessage> snapShotAction;

        public void SetID(byte id)
        {
            this.id = id;
            NetworkManager.OnMessageReceived += ReceiveData;
        }

        public byte GetId()
        {
            return id;
        }

        public void ReceiveData(NetworkMessage message)
        {
            switch(message.MessageType)
            {
                case MessageType.PlayerSnapShot:
                    PlayerSnapShot playerData = (PlayerSnapShot)message;
                    if(playerData.playerID == id)
                    {
                        snapShotAction?.Invoke(playerData);
                    }
                    break;
            }
        }
    }
}
