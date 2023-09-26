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
    }

    [MessagePackObject]
    public class Join : NetworkMessage
    {
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientHasJoined;
        //public override bool PriorityMessage { get; set; } = true;
        [Key(1)]
        public string playerName;
    }

    [MessagePackObject]
    public class Leave : NetworkMessage
    {
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientHasLeft;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class JoinAnswer : NetworkMessage
    {
        [Key(1)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ClientJoinAnswer;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerJoined : NetworkMessage
    {
        [Key(1)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerJoined;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class PlayerLeft : NetworkMessage
    {
        [Key(1)]
        public byte playerID;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.PlayerLeft;
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
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
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
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
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
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
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
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
        //public override bool PriorityMessage { get; set; } = false;
    }

    [MessagePackObject]
    public class ChatMessage : NetworkMessage
    {
        
        [Key(1)] 
        public string Message;
        [Key(2)] 
        public string UserName;
        [Key(3)] 
        public DateTime Time;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatMessage;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class ChatCommand : NetworkMessage
    {
        
        [Key(1)] 
        public Commands Command;
        [Key(2)] 
        public string? UserName;
        [Key(3)] 
        public DateTime Time;
        [Key(4)] 
        public string? TargetName;
        [Key(5)] 
        public string? Message;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatCommand;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class ChatAcknowledgement : NetworkMessage
    {
        [Key(1)]
        public byte playerID;
        [Key(2)]
        public MessageType OriginalMessageType;
        [Key(3)]
        public Guid MessageId;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.ChatAcknowledgement;
        //public override bool PriorityMessage { get; set; } = true;
    }

    [MessagePackObject]
    public class Acknowledgement : NetworkMessage
    {
        [Key(1)]
        public byte playerID;
        [Key(2)]
        public MessageType OriginalMessageType;
        [Key(3)]
        public Guid MessageId;
        [IgnoreMember]
        public override MessageType MessageType => MessageType.Acknowledgement;
        //public override bool PriorityMessage { get; set; } = true;
    }
}
