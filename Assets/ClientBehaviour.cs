using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;

public class ClientBehaviour : MonoBehaviour
{

    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public bool Done;

    void Start()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);

        //var endpoint = NetworkEndPoint.LoopbackIpv4;
        var endpoint = NetworkEndPoint.Parse("77.167.147.11", 9000);
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }


    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            if (!Done)
                Debug.Log("Something went wrong during connect");
            return;
        }
        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = m_Connection.PopEvent(m_Driver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("We are now connected to the server");

                //uint value = 1;
                //var writer = m_Driver.BeginSend(m_Connection);
                //writer.WriteUInt(value);
                //m_Driver.EndSend(writer);
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                //a data package came in!
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.newPlayer:
                        break;
                    case MessageHeader.MessageType.welcome:
                        var welcomeMessage = new WelcomeMessage();
                        welcomeMessage.DeserializeObject(ref reader);
                        //Debug.Log($"recieved message, ID : {welcomeMessage.header.ID} , player ID: {welcomeMessage.PlayerID}, Color: {welcomeMessage.Colour} |");

                        var setNameMessage = new SetNameMessage()
                        {
                            Name = "Casey krijgt een O"
                        };
                        var writer = m_Driver.BeginSend(m_Connection); 
                        setNameMessage.SerializeObject(ref writer);
                        m_Driver.EndSend(writer);

                        break;
                    case MessageHeader.MessageType.setName:
                        break;
                    case MessageHeader.MessageType.requestDenied:
                        break;
                    case MessageHeader.MessageType.playerLeft:
                        break;
                    case MessageHeader.MessageType.startGame:
                        break;
                    case MessageHeader.MessageType.none:
                    default:
                        break;
                }

                //uint value = stream.ReadUInt();
                //Debug.Log("Got the value = " + value + " back from the server");
                //Done = true;
                //m_Connection.Disconnect(m_Driver);
                //m_Connection = default(NetworkConnection);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }
        }
    }
}
