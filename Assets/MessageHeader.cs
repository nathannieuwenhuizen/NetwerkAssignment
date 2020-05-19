using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public interface IMessage
{
    void SerializeObject(ref DataStreamWriter writer);
    void DeserializeObject(ref DataStreamReader reader);
}

public struct MessageHeader : IMessage
{

    private static uint nextID = 0;
    private static uint NextID => ++nextID;
    public IMessage Message { get; set; }

    public enum MessageType
    {
        none = 0,
        newPlayer,
        welcome,
        setName,
        requestDenied,
        playerLeft,
        startGame,
        count
    }
    public MessageType Type { get; private set; }
    public uint ID { get; private set; }

    public void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteUShort((ushort)Type);
        writer.WriteUInt((uint)ID);

        Message.SerializeObject(ref writer);
    }
    public void DeserializeObject(ref DataStreamReader reader)
    {
        ID = reader.ReadUInt();

        Message.DeserializeObject(ref reader);
    }
}
