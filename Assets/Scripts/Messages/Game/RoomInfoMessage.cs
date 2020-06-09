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
    public List<int> OtherPlayerIDs { get; set; } = new List<int>();
    
    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(MoveDirections);
        writer.WriteUShort(TreasureRoom);
        writer.WriteByte(ContainsMonster);
        writer.WriteByte(ContainsExit);
        writer.WriteByte(NumberOfOtherPlayers);
        for(int i = 0; i < NumberOfOtherPlayers; i++)
        {
            writer.WriteInt(OtherPlayerIDs[i]);
        }
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        MoveDirections = reader.ReadByte();
        TreasureRoom = reader.ReadUShort();
        ContainsMonster = reader.ReadByte();
        ContainsExit = reader.ReadByte();
        NumberOfOtherPlayers = reader.ReadByte();

        OtherPlayerIDs = new List<int>();
        for (int i = 0; i < NumberOfOtherPlayers; i++)
        {
            OtherPlayerIDs.Add(reader.ReadInt());
        }
    }
}
