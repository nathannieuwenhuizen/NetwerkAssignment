using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;

public struct SetNameMessage: IMessage {

    public string Name { get; set; }

    public void SerializeObject(ref DataStreamWriter writer)
    {
        writer.WriteString(Name);
    }

    public void DeserializeObject(ref DataStreamReader reader)
    {
        Name = reader.ReadString().ToString();
    }
}
