using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;

//unity trnapsot doc: https://docs.unity3d.com/Packages/com.unity.transport@0.3/manual/workflow-client-server.html
public class ServerBehaviour : MonoBehaviour
{

    private NetworkDriver networkDriver;

    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    public Queue<MessageHeader> messagesQueue;

    public UnityEvent<MessageHeader>[] ServerCallbacks = new UnityEvent<MessageHeader>[(int)MessageHeader.MessageType.count - 1];

    // Start is called before the first frame update
    void Start()
    {
        networkDriver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4; //might use var instead
        endPoint.Port = 9000;

        if (networkDriver.Bind(endPoint) != 0)
        {
            Debug.Log("Failed to bind port to" + endPoint.Port);
        } else
        {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        messagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
        {
            ServerCallbacks[i] = new MessageEvent();
        }
        ServerCallbacks[(int)MessageHeader.MessageType.setName].AddListener(HandleSetName);
    }

    private void HandleSetName(MessageHeader message)
    {
        Debug.Log($"Got a name: {(message as SetNameMessage).Name}");
    }

    void Update()
    {
        networkJobHandle.Complete();

        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        NetworkConnection c;
        while ((c = networkDriver.Accept()) != default)
        {
            connections.Add(c);
            Debug.Log("Accepted connection");

            var colour = (Color32)Color.magenta;
            var message = new WelcomeMessage
            {
                PlayerID = c.InternalId,
                Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
            };

            var writer = networkDriver.BeginSend(c);
            message.SerializeObject(ref writer);
            networkDriver.EndSend(writer);
        }

        DataStreamReader reader;
        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                    switch (messageType)
                    {
                        case MessageHeader.MessageType.none:
                            break;
                        case MessageHeader.MessageType.newPlayer:
                            break;
                        case MessageHeader.MessageType.welcome:
                            break;
                        case MessageHeader.MessageType.setName:
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            messagesQueue.Enqueue(message);
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
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while (messagesQueue.Count > 0)
        {
            var message = messagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }

}
