using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public interface IMessage
{
    void SerializeObject(ref DataStreamWriter writer);
    void DeserializeObject(ref DataStreamReader reader);
}

public abstract class MessageHeader
{

    private static uint nextID = 0;
    public static uint NextID => ++nextID;

    public enum MessageType
    {
        none = 0, //lobby
        newPlayer,
        welcome,
        setName,
        requestDenied,
        playerLeft,
        startGame,
        count,
        playerTurn, //game
        roomInfo,
        playerEnterRoom,
        playerLeaveRoom,
        obtainTreasure,
        hitMonster,
        hitByMonster,
        playerDefends,
        playerLeftDungeon,
        playerDies,
        endGame,
        moveRequest,
        attackRequest,
        defendRequest,
        claimTreasureRequest,
        leaveDungeonRequest
    }

    public abstract MessageType Type { get; }
    public uint ID { get; private set; } = NextID;

    public virtual void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteUShort((ushort)Type);
        writer.WriteUInt(ID);
    }

    public virtual void DeserializeObject(ref DataStreamReader reader)
    {
        ID = reader.ReadUInt();
    }
}
