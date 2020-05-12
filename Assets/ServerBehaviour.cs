using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;


//unity trnapsot doc: https://docs.unity3d.com/Packages/com.unity.transport@0.3/manual/workflow-client-server.html
public class ServerBehaviour : MonoBehaviour
{

    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;

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
            var rc = (Color32)Random.ColorHSV();
            var message = new WelcomeMessage
            {
                PlayerID = c.InternalId,
                Colour = ((uint)rc.r << 24) | ((uint)rc.g << 16) |((uint)rc.r << 8) | ((uint)rc.a )
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
                    var messageType = (Message.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case Message.MessageType.newPlayer:
                            break;
                        case Message.MessageType.welcome:
                            break;
                        case Message.MessageType.setName:
                            Debug.Log("message is recieved");
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            Debug.Log($"Welcome player: {message.Name} !");
                            break;
                        case Message.MessageType.requestDenied:
                            break;
                        case Message.MessageType.playerLeft:
                            break;
                        case Message.MessageType.startGame:
                            break;
                        case Message.MessageType.none:
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
