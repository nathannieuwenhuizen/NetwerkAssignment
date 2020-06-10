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

    private ServerDataHolder serverDataHolder;

    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    public Queue<MessageHeader> messagesQueue;

    public UnityEvent<MessageHeader>[] ServerCallbacks = new UnityEvent<MessageHeader>[(int)MessageHeader.MessageType.count - 1];

    // Start is called before the first frame update
    void Start()
    {
        serverDataHolder = new ServerDataHolder();

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

    private bool HandleMoveRequest(MoverequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData previousRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        int[] nextRoom = serverDataHolder.GetNextRoomID(previousRoom, message.Direction);

        //check if player can move to that direction
        if (nextRoom == null)
        {
            //handle requestdenied message
            Debug.Log("Request denied"); 
            return false;
        }

        //if so update dataholder of server
        serverDataHolder.players.Find(x => x.playerIndex == connectID).roomID = nextRoom;

        //send room info to cplayer
        RoomInfoMessage newRoomMessage = serverDataHolder.GetRoomMessage(connectID);
        SendMessage(newRoomMessage, connections[connectID]);

        //send leavmessage to players who are in the previous room
        List<int> idsInPreviousRoom = serverDataHolder.GetPlayerIDsRoom(previousRoom);
        foreach(int id in idsInPreviousRoom) 
        {
            PlayerLeftRoom(connectID, id);
        }

        //send enter message to player who are in the next room.
        List<int> idsInNewRoom = serverDataHolder.GetPlayerIDsRoom(serverDataHolder.rooms[nextRoom[0], nextRoom[1]]);
        foreach (int id in idsInNewRoom)
        {
            PlayerJoinedRoom(connectID, id);
        }
        return true;
    }


    public void PlayerLeftRoom(int leftId, int recieverID)
    {
        PlayerLeaveRoomMessage message = new PlayerLeaveRoomMessage()
        {
            PlayerID = leftId
        };
        SendMessage(message, connections[recieverID]);
    }

    public void PlayerJoinedRoom(int joinID, int recieverID)
    {
        PlayerEnterRoomMessage message = new PlayerEnterRoomMessage()
        {
            PlayerID = joinID
        };
        SendMessage(message, connections[recieverID]);
    }

    private uint colorTouint(Color32 colour)
    {
        return ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a;
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

        NetworkConnection newConnection;
        while ((newConnection = networkDriver.Accept()) != default)
        {
            connections.Add(newConnection);
            Debug.Log("Accepted new connection");


            //new player data is set
            //Color col = Random.ColorHSV(); 
            Color col = ColorExtensions.colors[ (ColorExtensions.RandomStartIndex + newConnection.InternalId) % ColorExtensions.colors.Length];
            col.a = 1;
            var colour =(Color32)col;   
            var playerID = newConnection.InternalId;
            var welcomeMessage = new WelcomeMessage
            {

                PlayerID = playerID,
                Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
            };
            SendMessage(welcomeMessage, newConnection);

            //save it to list
            PlayerData newData = new PlayerData();
            newData.color = colour;
            newData.playerIndex = playerID;
            if (serverDataHolder.players == null) { serverDataHolder.players = new List<PlayerData>(); }
            serverDataHolder.players.Add(newData);

            Debug.Log("server data holder players count: " + serverDataHolder.players.Count);

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
                            StayAlive(i);
                            break;
                        case MessageHeader.MessageType.newPlayer: 
                            break;
                        case MessageHeader.MessageType.welcome:
                            break;
                        case MessageHeader.MessageType.setName:
                            var message = new SetNameMessage();
                            message.DeserializeObject(ref reader);
                            messagesQueue.Enqueue(message);

                            PlayerData newPlayerData = GetPlayerData(connections[i]);
                            newPlayerData.name = message.Name;

                            NewPlayerJoined(connections[i]);

                            break;
                        case MessageHeader.MessageType.requestDenied:
                            break;
                        case MessageHeader.MessageType.playerLeft:
                            break;
                        case MessageHeader.MessageType.moveRequest:
                            var moveRequest = new MoverequestMessage();
                            moveRequest.DeserializeObject(ref reader);
                            messagesQueue.Enqueue(moveRequest);
                            bool canmove = HandleMoveRequest(moveRequest, i);
                            if (canmove)
                            {
                                NextPlayerTurn();
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    PlayerLeftMessage playerLeftMessage = new PlayerLeftMessage
                    {
                        PlayerLeftID = i
                    };

                    SendMessageToAll(playerLeftMessage);
                    Debug.Log("Client disconnected");
                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    public void NextPlayerTurn()
    {
        serverDataHolder.turnID = (serverDataHolder.turnID + 1) % connections.Length;

        PlayerTurnMessage turnMessage = new PlayerTurnMessage
        {
            PlayerID = serverDataHolder.turnID
        };
        SendMessageToAll(turnMessage);
    }

    public void StartGame()
    {
        networkJobHandle.Complete();
        StartGameMessage startMessage = new StartGameMessage
        {
            StartHP = 10
        };
        SendMessageToAll(startMessage);

        //enviroment data is setup
        serverDataHolder.GameSetup();

        //everyone gets a rooms info message.
        for(int i = 0; i < connections.Length; i++)
        {
            RoomInfoMessage startRoomMessage = serverDataHolder.GetRoomMessage(i);
            SendMessage(startRoomMessage, connections[i]);
        }

        //turn event is called
        PlayerTurnMessage turnMessage = new PlayerTurnMessage
        {
            PlayerID = serverDataHolder.turnID
        };
        SendMessageToAll(turnMessage);

    }

    public void NewPlayerJoined(NetworkConnection newPlayerConnection)
    {
        PlayerData newPlayerData = GetPlayerData(newPlayerConnection);


        //send all players the info of new player
        NewPlayerMessage newPlayermessage = new NewPlayerMessage
        {
            PlayerID = newPlayerData.playerIndex,
            Colour = colorTouint((Color32)newPlayerData.color),
            PlayerName = newPlayerData.name
        };
        SendMessageToAll(newPlayermessage);


        //send the other player date to the new connection 
        foreach (NetworkConnection conn in connections)
        {
            if (conn == newPlayerConnection) return;

            PlayerData otherPlayerData = GetPlayerData(conn);
            if (otherPlayerData.name == "") return;

            NewPlayerMessage otherPlayerMessage = new NewPlayerMessage
            {
                PlayerID = otherPlayerData.playerIndex,
                Colour = colorTouint((Color32)otherPlayerData.color),
                PlayerName = otherPlayerData.name
            };
            SendMessage(otherPlayerMessage, newPlayerConnection);
        }

    }

    private void ProcessMessagesQueue()
    {
        while (messagesQueue.Count > 0)
        {
            var message = messagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    private PlayerData GetPlayerData(NetworkConnection connection)
    {
        foreach(PlayerData data in serverDataHolder.players)
        {
            if (data.playerIndex == connection.InternalId)
            {
                return data;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }

    private void StayAlive(int i)
    {
        Debug.Log("Server StayAliveSend");
        var noneMessage = new NoneMessage();

        SendMessage(noneMessage, connections[i]);
    }


    public NewPlayerMessage CreateNewPlayerMessage(NetworkConnection connection)
    {
        PlayerData newPlayerData = GetPlayerData(connection);

        NewPlayerMessage result = new NewPlayerMessage
        {
            PlayerID = newPlayerData.playerIndex,
            Colour = colorTouint((Color32)newPlayerData.color),
            PlayerName = newPlayerData.name
        };
        return result;
    }

    public void SendMessage(MessageHeader message, NetworkConnection connection)
    {
        var writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);

    }

    public void SendMessageToAll(MessageHeader message)
    {
        for(int i = 0; i < connections.Length; i++)
        {
            SendMessage(message, connections[i]);
        }
    }


}
