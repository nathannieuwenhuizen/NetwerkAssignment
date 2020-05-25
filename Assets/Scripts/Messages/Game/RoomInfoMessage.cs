using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class RoomInfoMessage : MessageHeader
{
    public override MessageType Type => MessageType.roomInfo;

    public byte MoveDirections { get; set; }
    public ushort TreasureRoom { get; set; }
    public byte ContainsMonster { get; set; }
    public byte ContainsExit { get; set; }
    public byte NumberOfOtherPlayers { get; set; }
    public int OtherPLayerIDs { get; set; }
    
    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(MoveDirections);
        writer.WriteUShort(TreasureRoom);
        writer.WriteByte(ContainsMonster);
        writer.WriteByte(ContainsExit);
        writer.WriteByte(NumberOfOtherPlayers);
        writer.WriteInt(OtherPLayerIDs);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        MoveDirections = reader.ReadByte();
        TreasureRoom = reader.ReadUShort();
        ContainsMonster = reader.ReadByte();
        ContainsExit = reader.ReadByte();
        NumberOfOtherPlayers = reader.ReadByte();
        OtherPLayerIDs = reader.ReadInt();
    }
}
