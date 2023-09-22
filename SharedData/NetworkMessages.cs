using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MessagePack;

namespace SharedData
{
    
        [MessagePackObject]
        public abstract class NetworkMessage
        {
            [IgnoreMember]
            public abstract MessageType MessageType { get; }
            [IgnoreMember]
            public byte GetMessageTypeAsByte => (byte)MessageType;
            [Key(0)]
            public bool PriorityMessage = false;
        }

        public class Join : NetworkMessage
        {
            [IgnoreMember]
            public override MessageType MessageType => MessageType.ClientHasJoined;
        }

        public class Leave : NetworkMessage
        {
            [IgnoreMember]
            public override MessageType MessageType => MessageType.ClientHasJoined;
        }

        public class JoinAnswer : NetworkMessage
        {
            [Key(1)]
            public byte playerID;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.ClientJoinAnswer;
        }

        public class PlayerJoined : NetworkMessage
        {
            [Key(1)]
            public byte playerID;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.PlayerJoined;
        }

        public class PlayerLeft : NetworkMessage
        {
            [Key(1)]
            public byte playerID;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.PlayerLeft;
        }

        public class PlayerSnapShot : NetworkMessage
        {
            [Key(1)]
            public int SnapSeqId;
            [Key(2)]
            public byte playerID;
            [Key(3)]
            public float positionX;
            [Key(4)]
            public float positionY;
            [Key(5)]
            public float positionZ;
            [Key(6)]
            public float rotZ;
            [Key(7)]
            public int health;  // Added health property
            [Key(8)]
            public int ammo;    // Added ammo property
            [IgnoreMember]
            public override MessageType MessageType => MessageType.PlayerSnapShot;
        }


        public class PlayerUpdate : NetworkMessage
        {
            [Key(1)]
            public int SnapSeqId;
            [Key(2)]
            public bool up;
            [Key(3)]
            public bool down;
            [Key(4)]
            public bool left;
            [Key(5)]
            public bool right;
            [Key(6)]
            public bool jump;
            [Key(7)]
            public bool shoot;
            [Key(8)]
            public bool reload;
            [Key(9)]
            public float rotZ;
            [Key(10)]
            public float rotY;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.PlayerUpdate;
        }

        public class PlayerInfoUpdate : NetworkMessage
        {
            [Key(1)]
            public int health;
            [Key(2)]
            public int ammo;
            [Key(3)]
            public int points;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.PlayerInfoUpdate;
        }

        public class LaserShot : NetworkMessage
        {
            [Key(1)]
            public float posX;
            [Key(2)]
            public float posY;
            [Key(3)]
            public float posZ;
            [Key(4)]
            public float rotX;
            [Key(5)]
            public float rotY;
            [Key(6)]
            public float rotZ;
            [Key(7)]
            public float length;
            [IgnoreMember]
            public override MessageType MessageType => MessageType.LaserShot;
        }    
}
