using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public struct WelcomeMessage : IMessage {

    public int PlayerID { get; set; }
    public uint Colour { get; set; }


    public void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteInt(PlayerID);
        writer.WriteUInt(Colour);
    }

    public void DeserializeObject(ref DataStreamReader reader)
    {
        PlayerID = reader.ReadInt();
        Colour = reader.ReadUInt();
    }
}
