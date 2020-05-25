using Unity.Networking.Transport;

public class EndGameMessage : MessageHeader
{
    public override MessageType Type => MessageType.endGame;

    public byte NumberOfScores { get; set; }
    public int PlayerIDHighscorePairs { get; set; } // vraag aan vincent hoe deze moet...

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(NumberOfScores);
        writer.WriteInt(PlayerIDHighscorePairs);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        NumberOfScores = reader.ReadByte();
        PlayerIDHighscorePairs = reader.ReadInt();
    }
}
