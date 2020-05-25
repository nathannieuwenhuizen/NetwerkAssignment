using Unity.Networking.Transport;

public class AttackRequestMessage : MessageHeader
{
    public override MessageType Type => MessageType.attackRequest;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
    }
}
