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
        dataHolder.client = this;

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
                        PlayerLeftLobby(ref reader);
                        break;
                    case MessageHeader.MessageType.startGame:
                        StartGame(ref reader);
                        break;
                    case MessageHeader.MessageType.roomInfo:
                        GetRoomInfo(ref reader);
                        break;
                    case MessageHeader.MessageType.playerEnterRoom:
                        PlayerEnterRoom(ref reader);
                        break;
                    case MessageHeader.MessageType.playerLeaveRoom:
                        PlayerLeaveRoom(ref reader);
                        break;
                    case MessageHeader.MessageType.playerTurn:
                        PlayerTurn(ref reader);
                        break;
                    case MessageHeader.MessageType.obtainTreasure:
                        ObtainTreasure(ref reader);
                        break;
                    case MessageHeader.MessageType.playerLeftDungeon:
                        PlayerLeftDungeon(ref reader);
                        break;
                    case MessageHeader.MessageType.endGame:
                        EndGame(ref reader);
                        break;
                    case MessageHeader.MessageType.hitMonster:
                        HitMonster(ref reader);
                        break;
                    case MessageHeader.MessageType.hitByMonster:
                        HitByMonster(ref reader);
                        break;
                    case MessageHeader.MessageType.playerDies:
                        PlayerDies(ref reader);
                        break;
                    case MessageHeader.MessageType.playerDefends:
                        DefendsAgainstMonster(ref reader);
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

    private void PlayerEnterRoom(ref DataStreamReader reader)
    {
        var message = new PlayerEnterRoomMessage();
        message.DeserializeObject(ref reader);
        dataHolder.game.cRoom.PlayerJoinedRoom(dataHolder.players.Find(x => x.playerIndex == message.PlayerID));
        dataHolder.game.cRoom.UpdatePlayerUIElements();

    }
    private void PlayerLeaveRoom(ref DataStreamReader reader)
    {
        var message = new PlayerLeaveRoomMessage();
        message.DeserializeObject(ref reader);
        Debug.Log("message id: " + message.PlayerID);
        //Debug.Log("message id: " + );
        dataHolder.game.cRoom.PlayerLeftRoom(dataHolder.players.Find(x => x.playerIndex == message.PlayerID));
        dataHolder.game.cRoom.UpdatePlayerUIElements();
    }

    private void PlayerTurn(ref DataStreamReader reader)
    {
        var message = new PlayerTurnMessage();
        message.DeserializeObject(ref reader);
        dataHolder.game.PlayerTurn(message.PlayerID);
    }

    private void WelcomeFromServer(ref DataStreamReader reader)
    {
        var welcomeMessage = new WelcomeMessage();
        welcomeMessage.DeserializeObject(ref reader);
        dataHolder.myData.playerIndex = welcomeMessage.PlayerID;
        dataHolder.myData.color = UIntToColor(welcomeMessage.Colour);
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


    public void SendMoveRequest(DirectionEnum Enum)
    {
        var moveRequest = new MoverequestMessage();
        moveRequest.Direction = (byte)Enum;
        SendMessage(moveRequest);
    }


    private void PlayerLeftLobby(ref DataStreamReader reader)
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
         
        if (newData.playerIndex == dataHolder.myData.playerIndex)
        {
            dataHolder.myData.name = newData.name;
        } 
        dataHolder.players.Add(newData);
        dataHolder.lobby.UpdateLobby(dataHolder.players.ToArray());

    }


    public void HitMonster(ref DataStreamReader reader) 
    {
        var message = new HitMonsterMessage();
        message.DeserializeObject(ref reader);
        dataHolder.game.cRoom.roomData.monsterHP -= message.damageDealt;
        Debug.Log("Hitting monster...");
        if (dataHolder.game.cRoom.roomData.monsterHP <= 0)
        {
            dataHolder.game.cRoom.roomData.containsMonster = false;
        }
        dataHolder.game.cRoom.UpdateHPText();
        dataHolder.game.cRoom.UpdateRoom();
    } 

    public void HitByMonster(ref DataStreamReader reader)
    {
        var message = new HitByMonsterMessage();
        message.DeserializeObject(ref reader);

        if (message.PlayerID == dataHolder.myData.playerIndex)
        {
            dataHolder.myData.hp = message.newHP;
            dataHolder.game.cRoom.UpdateHPText();
        }
    }

    public void PlayerDies(ref DataStreamReader reader) // is ok, but needs die animation?
    {
        var message = new PlayerDiesMessage();
        message.DeserializeObject(ref reader);

        if (message.PlayerID == dataHolder.myData.playerIndex)
        {
            dataHolder.game.cRoom.GameOver();
        } else
        {
            dataHolder.game.cRoom.PlayerLeftRoom(dataHolder.players.Find(x => x.playerIndex == message.PlayerID));
        }
        dataHolder.game.cRoom.UpdatePlayerUIElements(); 
    }


    public void DefendsAgainstMonster(ref DataStreamReader reader)
    {
        var message = new PlayerDefendsMessage();
        message.DeserializeObject(ref reader);
        
        if (message.PlayerID == dataHolder.myData.playerIndex)
        {
            dataHolder.myData.hp = message.newHP;
            dataHolder.game.cRoom.UpdateHPText();
        }
    }



    public void PlayerLeftDungeon(ref DataStreamReader reader)
    {
        var message = new PLayerLeftDungeonMessage();
        message.DeserializeObject(ref reader);

        //I left dungeon
        if(message.PlayerID == dataHolder.myData.playerIndex)
        {
            dataHolder.game.cRoom.ILeftDungeon();
        }
        else
        {
            PlayerData leftPlayer = dataHolder.game.cRoom.otherPlayersInRoom.Find(x => x.playerIndex == message.PlayerID);

            //Other player left dungeon
            //PlayerData leftPlayer = dataHolder.players.Find(x => x.playerIndex == message.PlayerID);
            if (leftPlayer != null)
            {
                Debug.Log("left player: " + leftPlayer);
                dataHolder.game.cRoom.PlayerLeftRoom(leftPlayer);
            }
        }

        //update ui
        dataHolder.game.cRoom.UpdatePlayerUIElements();
    }

    public void EndGame(ref DataStreamReader reader)
    {
        var message = new EndGameMessage();
        message.DeserializeObject(ref reader);

        HighScorePair[] endScores = message.PlayerIDHighscorePairs;

        dataHolder.ShowEndScreen(message);

    }

    private void ObtainTreasure(ref DataStreamReader reader)
    {
        var message = new ObtainTreasureMessage();
        message.DeserializeObject(ref reader);

        dataHolder.myData.score += message.Amount;

        dataHolder.game.cRoom.treasure.obj.SetActive(false);
        dataHolder.game.cRoom.treasure.SetButtons(false);
        //todo: update score ui?
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
        //Debug.Log("Client StayAliveSend");
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