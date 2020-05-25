using Unity.Networking.Transport;

public class MoverequestMessage : MessageHeader
{
    public override MessageType Type => MessageType.moveRequest;

    public byte Direction { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(Direction);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        Direction = reader.ReadByte();
    }
}
