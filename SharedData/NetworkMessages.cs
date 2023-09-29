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
        public bool PriorityMessage { get; set; }
        [Key(1)]
        public Guid MessageId;
    }

    [MessagePackObject]
    public class ClientHasJoined : NetworkMessage
    {
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientHasJoined;
        //public override bool PriorityMessage { get; set; } = true;
        [Key(3)]
        public string playerName;
    }

    [MessagePackObject]
    public class ClientJoinAnswer : NetworkMessage
    {
        [Key(3)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientJoinAnswer;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class ClientHasLeft : NetworkMessage
    {
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientHasLeft;
        //public override bool PriorityMessage { get; set; } = false;
    }
    
    [MessagePackObject]
    public class ServerInfoMessage : NetworkMessage
    {
        [Key(1)]
        public string ServerInformation;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ServerInfoMessage;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerJoined : NetworkMessage
    {
        [Key(3)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerJoined;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerLeft : NetworkMessage
    {
        [Key(3)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerLeft;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerSnapShot : NetworkMessage
    {
        [Key(2)]
        public int SnapSeqId;
        [Key(3)]
        public byte playerID;
        [Key(4)]
        public float positionX;
        [Key(5)]
        public float positionY;
        [Key(6)]
        public float positionZ;
        [Key(7)]
        public float rotZ;
        [Key(8)]
        public int health;  // Added health property
        [Key(9)]
        public int ammo;    // Added ammo property
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerSnapShot;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerUpdate : NetworkMessage
    {
        [Key(2)]
        public int SnapSeqId;
        [Key(3)]
        public bool up;
        [Key(4)]
        public bool down;
        [Key(5)]
        public bool left;
        [Key(6)]
        public bool right;
        [Key(7)]
        public bool jump;
        [Key(8)]
        public bool shoot;
        [Key(9)]
        public bool reload;
        [Key(10)]
        public float rotZ;
        [Key(11)]
        public float rotY;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerUpdate;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerInfoUpdate : NetworkMessage
    {
        [Key(2)]
        public int health;
        [Key(3)]
        public int ammo;
        [Key(4)]
        public int points;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerInfoUpdate;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class LaserShot : NetworkMessage
    {
        [Key(2)]
        public float posX;
        [Key(3)]
        public float posY;
        [Key(4)]
        public float posZ;
        [Key(5)]
        public float rotX;
        [Key(6)]
        public float rotY;
        [Key(7)]
        public float rotZ;
        [Key(8)]
        public float length;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.LaserShot;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class ChatMessage : NetworkMessage
    {
        
        [Key(2)] 
        public string Message;
        [Key(3)] 
        public string UserName;
        [Key(4)] 
        public DateTime Time;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatMessage;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class ChatCommand : NetworkMessage
    {
        
        [Key(2)] 
        public Commands Command;
        [Key(3)] 
        public string? UserName;
        [Key(4)] 
        public DateTime Time;
        [Key(5)] 
        public string? TargetName;
        [Key(6)] 
        public string? Message;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatCommand;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class ChatAcknowledgement : NetworkMessage
    {
        [Key(2)]
        public byte playerID;
        [Key(3)]
        public MessageType OriginalMessageType;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatAcknowledgement;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class Acknowledgement : NetworkMessage
    {
        [Key(2)]
        public byte playerID;
        [Key(3)]
        public MessageType OriginalMessageType;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.Acknowledgement;
        //public override bool PriorityMessage { get; set; } = true;
    }
}
