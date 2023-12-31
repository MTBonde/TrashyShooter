﻿namespace SharedData
{
    public enum MessageType : byte
    {
        // Connection
        ClientHasJoined = 0,  // 0000
        ClientJoinAnswer = 1,  // 0001
        ClientHasLeft = 2,  // 0010
        ServerInfoMessage = 3,  // 0011

        // Player
        PlayerJoined = 4,  // 0100
        PlayerLeft = 5,  // 0101
        PlayerUpdate = 6,  // 0110
        PlayerSnapShot = 7,  // 0111
        PlayerInfoUpdate = 8,  // 1000

        // Visual
        LaserShot = 9,  // 1001

        // Communication
        ChatMessage = 10,  // 1010
        ChatCommand = 11,  // 1011
        ChatAcknowledgement = 12,  // 1100

        // Error
        Error = 13,  // 1101

        // Reserved
        Acknowledgement = 14,  // 1110
        res4 = 15,  // 1111
    }

    public enum MessagePriority : byte
    {
        Low = 0,
        High = 1
    }

    public enum Commands
    {
        List, 
        All, 
        Direct
    }
}
