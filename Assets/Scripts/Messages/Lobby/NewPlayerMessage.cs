using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Networking.Transport;

public class NewPlayerMessage : MessageHeader
{
    public override MessageType Type => MessageType.newPlayer;

    public int PlayerID { get; set; }
    public uint Colour { get; set; }
    public string PlayerName { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUInt(Colour);
        writer.WriteString(PlayerName);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        Colour = reader.ReadUInt();
        PlayerName = reader.ReadString();
    }
}
