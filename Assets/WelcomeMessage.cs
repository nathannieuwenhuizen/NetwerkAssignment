using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class WelcomeMessage : Message {

    public int PlayerID { get; set; }
    public uint Colour { get; set; }

    public WelcomeMessage()
    {
        Type = MessageType.welcome;
    }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUInt(Colour);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        Colour = reader.ReadUInt();
    }
}
