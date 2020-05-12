using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public class Message
{

    private static uint nextID = 0;
    private static uint NextID => ++nextID;


    public enum MessageType
    {
        none = 0,
        newPlayer,
        welcome,
        setName,
        requestDenied,
        playerLeft,
        startGame
    }
    public MessageType Type { get; protected set; }
    public uint ID { get; private set; } = NextID;

    public virtual void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteUShort((ushort)Type);
        writer.WriteUInt((uint)ID);
    }
    public virtual void DeserializeObject(ref DataStreamReader reader)
    {
        ID = reader.ReadUInt();
    }
}
