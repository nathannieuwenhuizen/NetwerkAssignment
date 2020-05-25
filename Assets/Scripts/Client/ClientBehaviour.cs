using UnityEngine;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Unity.Jobs;

public class ClientBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NetworkConnection connection;

    private JobHandle networkJobHandle;

    // Use this for initialization
    void Start()
    {
        networkDriver = NetworkDriver.Create();
        connection = default;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        //var endpoint = NetworkEndPoint.Parse("77.167.147.11", 9000);
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
    }

    // Update is called once per frame
    void Update()
    {
        networkJobHandle.Complete();

        if (!connection.IsCreated)
        {
            return;
        }

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.none:
                        break;
                    case MessageHeader.MessageType.newPlayer:
                        break;
                    case MessageHeader.MessageType.welcome:
                        var welcomeMessage = new WelcomeMessage();
                        welcomeMessage.DeserializeObject(ref reader);

                        Debug.Log("Got a welcome message");

                        var setNameMessage = new SetNameMessage
                        {
                            Name = "Vincent"
                        };
                        var writer = networkDriver.BeginSend(connection);
                        setNameMessage.SerializeObject(ref writer);
                        networkDriver.EndSend(writer);
                        break;
                    case MessageHeader.MessageType.setName:
                        break;
                    case MessageHeader.MessageType.requestDenied:
                        break;
                    case MessageHeader.MessageType.playerLeft:
                        break;
                    case MessageHeader.MessageType.startGame:
                        break;
                    case MessageHeader.MessageType.count:
                        break;
                    default:
                        break;
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
    }
}