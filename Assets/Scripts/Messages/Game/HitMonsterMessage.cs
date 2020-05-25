using Unity.Networking.Transport;

public class HitByMonsterMessage : MessageHeader
{
    public override MessageType Type => MessageType.hitMonster;

    public int PlayerID { get; set; }
    public ushort damageDealt { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUShort(damageDealt);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        damageDealt = reader.ReadUShort();
    }
}
