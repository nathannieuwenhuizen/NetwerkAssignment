using Unity.Networking.Transport;

public class HitMonsterMessage : MessageHeader
{
    public override MessageType Type => MessageType.hitByMonster;

    public int PlayerID { get; set; }
    public ushort newHP { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUShort(newHP);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        newHP = reader.ReadUShort();
    }
}
