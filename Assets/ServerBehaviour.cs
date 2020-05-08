using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Networking.Transport;
using Unity.Collections;


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
        }

        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
                continue;
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    uint number = stream.ReadUInt();
                    Debug.Log("Got " + number + " from the Client adding + 2 to it.");

                    number += 2;

                    var writer = m_Driver.BeginSend(NetworkPipeline.Null, m_Connections[i]);
                    writer.WriteUInt(number);
                    m_Driver.EndSend(writer);
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
