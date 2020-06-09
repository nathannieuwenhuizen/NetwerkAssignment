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

    private float timePassed = 0;
    private float aliveDuration = 10;

    public DataHolder dataHolder;

    void Start()
    {
        dataHolder = GetComponent<DataHolder>();

        networkDriver = NetworkDriver.Create();
        connection = default;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        //var endpoint = NetworkEndPoint.Parse("77.167.147.11", 9000);
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
    }

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
                        PlayerJoined(ref reader);
                        break;
                    case MessageHeader.MessageType.welcome:
                        WelcomeFromServer(ref reader);
                        break;
                    case MessageHeader.MessageType.requestDenied:
                        break;
                    case MessageHeader.MessageType.playerLeft:
                        PlayerLeft(ref reader);
                        break;
                    case MessageHeader.MessageType.startGame:
                        StartGame(ref reader);
                        break;
                    case MessageHeader.MessageType.roomInfo:
                        GetRoomInfo(ref reader);
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
        CheckAliveSend();

        networkJobHandle = networkDriver.ScheduleUpdate();
    }
     
    private void WelcomeFromServer(ref DataStreamReader reader)
    {
        var welcomeMessage = new WelcomeMessage();
        welcomeMessage.DeserializeObject(ref reader);
        dataHolder.myData.playerIndex = welcomeMessage.PlayerID;
        dataHolder.myData.color = UIntToColor(welcomeMessage.Colour);

        Debug.Log("Got a welcome message");
    }

    private void GetRoomInfo(ref DataStreamReader reader)
    {
        var roomInfoMessage = new RoomInfoMessage();
        roomInfoMessage.DeserializeObject(ref reader);

        dataHolder.game.UpdateRoom(roomInfoMessage);
    }


    private void StartGame(ref DataStreamReader reader)
    {
        var startGameMessage = new StartGameMessage();
        startGameMessage.DeserializeObject(ref reader);
        dataHolder.myData.hp = startGameMessage.StartHP;
        dataHolder.StartGame();
    }



    private void PlayerLeft(ref DataStreamReader reader)
    {
        var playerLeftMessage = new PlayerLeftMessage();
        playerLeftMessage.DeserializeObject(ref reader);

        PlayerData removedData = dataHolder.players.Find(x => x.playerIndex == playerLeftMessage.PlayerLeftID);
        dataHolder.players.Remove(removedData);
        dataHolder.lobby.UpdateLobby(dataHolder.players.ToArray());
    }
    private void PlayerJoined(ref DataStreamReader reader)
    {
        var message = new NewPlayerMessage();
        message.DeserializeObject(ref reader);
        PlayerData newData = new PlayerData
        {
            playerIndex = message.PlayerID,
            color = UIntToColor(message.Colour),
            name = message.PlayerName
        };
        dataHolder.players.Add(newData);
        dataHolder.lobby.UpdateLobby(dataHolder.players.ToArray());
    }

    private void CheckAliveSend()
    {
        timePassed += Time.deltaTime;
        if (timePassed > aliveDuration)
        {
            timePassed = 0;
            StayAlive();
        }
    } 

    private Color UIntToColor(uint color)
    {
        float r = (byte)(color >> 24);
        float g = (byte)(color >> 16);
        float b = (byte)(color >> 8);
        float a = (byte)(color >> 0);
        //Debug.Log("uint = " + color);
        //Debug.Log("r = " + r);
        //Debug.Log("g = " + g);
        //Debug.Log("b = " + b);
        //Debug.Log("a = " + a);

        return new Color(r / 255, g / 255, b / 255, a / 255);
    }


    private void OnDestroy()
    {
        networkJobHandle.Complete();

        networkDriver.Dispose();
    }


    private void StayAlive()
    {
        Debug.Log("Client StayAliveSend");
        var noneMessage = new NoneMessage();

        SendMessage(noneMessage);
    }

    public void SendMessage(MessageHeader message)
    {
        networkJobHandle.Complete();

        var writer = networkDriver.BeginSend(connection);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
    }
}