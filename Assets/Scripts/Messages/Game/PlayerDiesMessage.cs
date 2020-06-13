using Unity.Networking.Transport;

public class PlayerDiesMessage : MessageHeader
{
    public override MessageType Type => MessageType.playerDies;

    public int PlayerID { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
    }
}
