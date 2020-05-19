using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Events;


//unity trnapsot doc: https://docs.unity3d.com/Packages/com.unity.transport@0.3/manual/workflow-client-server.html
public class ServerBehaviour : MonoBehaviour
{

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

    public NativeQueue<MessageHeader> messagesQueue;

    public UnityEvent<MessageHeader>[] ServerCallbacks = new UnityEvent<MessageHeader>[(int)MessageHeader.MessageType.count - 1];

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4; //might use var instead
        endPoint.Port = 9000;

        if (m_Driver.Bind(endPoint) != 0)
        {
            Debug.Log("Failed to bind port to" + endPoint.Port);
        } else
        {
            m_Driver.Listen();
        }

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

    }

    private void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    private void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        //dequeue messages
        while (messagesQueue.Count > 0)
        {
            MessageHeader message = messagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {
                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            Debug.Log("Accepted a connection");

            //player recieves welcome message
            var rc = (Color32)UnityEngine.Random.ColorHSV();
            var message = new WelcomeMessage
            {
                PlayerID = c.InternalId,
                Colour = ColorExtensions.ToUInt(rc) // ((uint)rc.r << 24) | ((uint)rc.g << 16) |((uint)rc.r << 8) | ((uint)rc.a )
            };
            var writer = m_Driver.BeginSend(c);
            message.SerializeObject(ref writer);
            m_Driver.EndSend(writer);
        }

        DataStreamReader reader;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case MessageHeader.MessageType.newPlayer:
                            break;
                        case MessageHeader.MessageType.welcome:
                            break;
                        case MessageHeader.MessageType.setName:

                            var header = new MessageHeader();
                            header.Message = new SetNameMessage();
                            header.DeserializeObject(ref reader);
                            messagesQueue.Enqueue(header);

                            Debug.Log($"Welcome player: {((SetNameMessage)header.Message).Name} !");
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

                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    m_Connections[i] = default(NetworkConnection);
                }
            }
        }
    }
}
