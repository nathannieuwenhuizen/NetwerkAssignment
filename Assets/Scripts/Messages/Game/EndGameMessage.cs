using Unity.Networking.Transport;

public class EndGameMessage : MessageHeader
{
    public override MessageType Type => MessageType.endGame;

    public byte NumberOfScores { get; set; }
    //public int PlayerIDHighscorePairs { get; set; } // vraag aan vincent hoe deze moet...
    //public List<HighScorePair> OtherPlayerIDs { get; set; } = new List<HighScorePair>();
    public HighScorePair[] PlayerIDHighscorePairs { get; set; }

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteByte(NumberOfScores);
        for (int i = 0; i < NumberOfScores; i++)
        {
            writer.WriteInt(PlayerIDHighscorePairs[i].playerID);
            writer.WriteUShort(PlayerIDHighscorePairs[i].score);
        }
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        NumberOfScores = reader.ReadByte();
        PlayerIDHighscorePairs = new HighScorePair[NumberOfScores];
        for (int i = 0; i < NumberOfScores; i++)
        {
            PlayerIDHighscorePairs[i].playerID = reader.ReadInt();
            PlayerIDHighscorePairs[i].score = reader.ReadUShort();
        } 
    }
}

public struct HighScorePair
{
    public int playerID;
    public ushort score;
}
