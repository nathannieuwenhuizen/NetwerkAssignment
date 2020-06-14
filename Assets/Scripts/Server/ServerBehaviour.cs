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

        if (!Tools.LOCAL)
        {
            endPoint = NetworkEndPoint.Parse(Tools.IP, 9000);
        }

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
        //Debug.Log($"Got a name: {(message as SetNameMessage).Name}");
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
            if (connections.Length >= 4) {
                return;
            }
            connections.Add(newConnection);
            //Debug.Log("Accepted new connection");


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

            //Debug.Log("server data holder players count: " + serverDataHolder.players.Count);

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
                        case MessageHeader.MessageType.playerLeft:
                            break;
                        case MessageHeader.MessageType.moveRequest:
                            var moveRequest = new MoverequestMessage();
                            moveRequest.DeserializeObject(ref reader);
                            //messagesQueue.Enqueue(moveRequest);
                            bool canmove = HandleMoveRequest(moveRequest, i);
                            if (canmove)
                            {
                                NextPlayerTurn();
                            }
                            break;
                        case MessageHeader.MessageType.claimTreasureRequest:

                            var treasureRquest = new ClaimTreasureRequestMessage();
                            treasureRquest.DeserializeObject(ref reader);
                            HandleTreasureClaim(treasureRquest, i);
                            break;

                        case MessageHeader.MessageType.leaveDungeonRequest:
                            var leaveDungeonRequest = new LeavesDungeonRequestMessage();
                            leaveDungeonRequest.DeserializeObject(ref reader);
                            HandleLeaveDungeonRequest(leaveDungeonRequest, i);
                            break;

                        case MessageHeader.MessageType.defendRequest:
                            var defendRequest = new DefendRequestMessage();
                            defendRequest.DeserializeObject(ref reader);
                            HandleDefendRequest(defendRequest, i);
                            break;

                        case MessageHeader.MessageType.attackRequest:
                            var attackRequest = new AttackRequestMessage();
                            attackRequest.DeserializeObject(ref reader);
                            HandleAttackRequest(attackRequest, i);
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
    public void HandleLeaveDungeonRequest(LeavesDungeonRequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        Debug.Log("leave dungeon id = " + connectID);

        //check if player can leave dungeon
        if (!currentRoom.containsExit)
        {
            RequestDenied(message, connectID);
            return;
        }

        Debug.Log("leave ID = " + connectID);

        //edit the list
        serverDataHolder.activePlayerIDs.Remove(connectID); //needs testing
        playerData.activeInDungeon = false;

        //send message to all players the player left dungeon
        PLayerLeftDungeonMessage leftMessage = new PLayerLeftDungeonMessage()
        {
            PlayerID = connectID
        };
        SendMessageToAll(leftMessage);

        //next turn
        NextPlayerTurn();
    }

    //called when a request isnt possible
    private void RequestDenied(MessageHeader message, int connectID)
    {
        RequestDeniedMessage deniedMessage = new RequestDeniedMessage()
        {
            DeniedMessageID = message.ID
        };
        SendMessage(deniedMessage, connections[connectID]);
    }
    public void HandleTreasureClaim(ClaimTreasureRequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        //check treasure even has ammount
        if (currentRoom.treasureAmmount <= 0)
        {
            //if not then send request denied
            RequestDenied(message, connectID);
            return;
        }

        //then update treasure amount in room and transfer to player data
        int gainedTreasure = currentRoom.treasureAmmount;
        currentRoom.treasureAmmount = 0;

        //send obtain message back to all players in room
        List<int> ids = serverDataHolder.GetOtherPlayerIDsInSameRoom(playerData);
        ids.Add(connectID);
        Debug.Log("Other ids in room  count "+ ids.Count);
        ObtainTreasureMessage obtainMessage = new ObtainTreasureMessage()
        {
            Amount = (ushort)(gainedTreasure / ids.Count)
        };
        foreach (int id in ids)
        {
            serverDataHolder.players.Find(x => x.playerIndex == id).score += gainedTreasure / ids.Count;
            SendMessage(obtainMessage, connections[serverDataHolder.players.Find(x => x.playerIndex == id).playerIndex]);
        }

        //next turn
        NextPlayerTurn();
    }

    private bool HandleMoveRequest(MoverequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        int[] nextRoom = serverDataHolder.GetNextRoomID(currentRoom, message.Direction);

        bool monsterActive = currentRoom.containsMonster && currentRoom.monsterHP > 0;
        //check if player can move to that direction and if the monster in current room is killed or doesnt exist
        if (nextRoom == null || monsterActive )
        {
            //handle requestdenied message
            Debug.LogWarning("Request denied");
            return false;
        }

        //if so update dataholder of server
        serverDataHolder.players.Find(x => x.playerIndex == connectID).roomID = nextRoom;

        //send room info to cplayer
        RoomInfoMessage newRoomMessage = serverDataHolder.GetRoomMessage(connectID);
        SendMessage(newRoomMessage, connections[connectID]);

        //send leavmessage to players who are in the previous room
        List<int> idsInPreviousRoom = serverDataHolder.GetPlayerIDsRoom(currentRoom);
        foreach (int id in idsInPreviousRoom)
        {
            if (id != connectID)
            {
                //Debug.Log("player # " + connectID + " leaves room where player # " + id + " resides ");
                PlayerLeftRoom(connectID, id);
            }
        }

        //send enter message to player who are in the next room.
        List<int> idsInNewRoom = serverDataHolder.GetPlayerIDsRoom(serverDataHolder.rooms[nextRoom[0], nextRoom[1]]);
        foreach (int id in idsInNewRoom)
        {
            if (id != connectID)
            {
                //Debug.Log("player # " + connectID + " enter room where player # " + id + " resides ");
                PlayerJoinedRoom(connectID, id);
            }
        }
        return true;
    }

    public void HandleAttackRequest(AttackRequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        //monster adds player to target list
        Monster monster = serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID);
        if (!monster.targetPlayers.Contains(playerData.playerIndex))
        {
            monster.targetPlayers.Add(playerData.playerIndex);
        }


        //check treasure even has ammount
        if (currentRoom.containsMonster == false)
        {
            //if not then send request denied
            RequestDenied(message, connectID);
            return;
        }

        //then update data in room
        currentRoom.monsterHP -= 6;
        if (currentRoom.monsterHP <= 0)
        {
            currentRoom.containsMonster = false;
            serverDataHolder.activeMonsters.Remove(serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID));
        }

        //send hit message back to all players in room
        List<int> ids = serverDataHolder.GetOtherPlayerIDsInSameRoom(playerData);
        ids.Add(connectID);
        HitMonsterMessage hitMessage = new HitMonsterMessage()
        {
            PlayerID = connectID,
            damageDealt = 6
        };
        foreach (int id in ids)
        {
            SendMessage(hitMessage, connections[serverDataHolder.players.Find(x => x.playerIndex == id).playerIndex]);
        }

        NextPlayerTurn();
    }

    public void HandleDefendRequest(DefendRequestMessage message, int connectID)
    {
        PlayerData playerData = serverDataHolder.players.Find(x => x.playerIndex == connectID);
        RoomData currentRoom = serverDataHolder.rooms[playerData.roomID[0], playerData.roomID[1]];

        //monster adds player to target list
        Monster monster = serverDataHolder.activeMonsters.Find(x => x.roomID == playerData.roomID);
        if (!monster.targetPlayers.Contains(playerData.playerIndex))
        {
            monster.targetPlayers.Add(playerData.playerIndex);
        }

        //check treasure even has ammount
        if (currentRoom.containsMonster == false)
        {
            //if not then send request denied
            RequestDenied(message, connectID);
            return;
        }

        //then update data in player
        playerData.hp += 4;

        //send  message back to player
        PlayerDefendsMessage defendMessage = new PlayerDefendsMessage()
        {
            PlayerID = connectID,
            newHP = (ushort)playerData.hp
        };
        SendMessage(defendMessage, connections[connectID]);

        NextPlayerTurn();
    }

    public IEnumerator MonsterAttacks(Monster monster)
    {
        networkJobHandle.Complete();

        //get room
        RoomData currentRoom = serverDataHolder.rooms[monster.roomID[0], monster.roomID[1]];

        //get players insideof room 
        List<int> playerIDs = monster.targetPlayers;

        Debug.Log("Monster attack!" + playerIDs.Count);

        if (playerIDs.Count != 0)
        {
            yield return new WaitForSeconds(.1f);
            networkJobHandle.Complete();

            int randomIndex = Mathf.FloorToInt(Random.Range(0, playerIDs.Count));

            PlayerData data = serverDataHolder.players.Find(x => x.playerIndex == monster.targetPlayers[randomIndex]);

            data.hp -= 3;
            Debug.Log("random index = " +randomIndex+  "hp" + data.hp);
            Debug.Log(" id = " +playerIDs[randomIndex]);
            Debug.Log("target id = " + monster.targetPlayers[randomIndex]);
             
            if (data.hp <= 0) 
            {
                data.hp = 0;
                //player dies 
                PlayerDiesMessage dieMessage = new PlayerDiesMessage()
                {
                    PlayerID = monster.targetPlayers[randomIndex]
                };
                monster.targetPlayers.Remove(monster.targetPlayers[randomIndex]);

                serverDataHolder.activePlayerIDs.Remove(data.playerIndex);
                if (serverDataHolder.activePlayerIDs.Count == 0)
                {
                    EndGame();
                }
                SendMessageToAll(dieMessage);
            }
            else
            {
                HitByMonsterMessage hitByMessage = new HitByMonsterMessage()
                {
                    PlayerID = randomIndex,
                    newHP = (ushort)data.hp
                };

                foreach (int id in playerIDs)
                {
                    SendMessage(hitByMessage, connections[id]);
                }
            }
        }
    }

    public void PlayerDies()
    {

    }

    public void NextPlayerTurn()
    {
        StartCoroutine(Turning());
    }
    public IEnumerator Turning()
    {

        yield return new WaitForFixedUpdate();
        networkJobHandle.Complete();

        //check if all player left dungeon
        Debug.Log("next player turn...amount of active players" + serverDataHolder.activePlayerIDs.Count);
        if (serverDataHolder.activePlayerIDs.Count == 0)
        {
            //endgame!
            EndGame();
        }
        else
        {

            serverDataHolder.turnID += 1;

            if (serverDataHolder.turnID >= serverDataHolder.activePlayerIDs.Count)
            {
                Debug.Log("end of loop.");
                Debug.Log(serverDataHolder.activeMonsters.Count);

                //monsters now attack!
                foreach (Monster monster in serverDataHolder.activeMonsters)
                {
                    yield return StartCoroutine(MonsterAttacks(monster));
                    networkJobHandle.Complete();
                }
                serverDataHolder.turnID = 0;
            }
            
            if (serverDataHolder.activePlayerIDs.Count > serverDataHolder.turnID)
            {
                PlayerTurnMessage turnMessage = new PlayerTurnMessage
                {
                    PlayerID = serverDataHolder.activePlayerIDs[serverDataHolder.turnID]
                };
                SendMessageToAll(turnMessage);
            } 
        }
    }

    public void EndGame()
    {
        HighScorePair[] highScorePairs = new HighScorePair[serverDataHolder.players.Count];
        for(int i = 0; i < highScorePairs.Length; i++)
        {
            highScorePairs[i].playerID = serverDataHolder.players[i].playerIndex;
            highScorePairs[i].score = (ushort)serverDataHolder.players[i].score;
        }
        EndGameMessage message = new EndGameMessage()
        {
            NumberOfScores = (byte)highScorePairs.Length,
            PlayerIDHighscorePairs = highScorePairs
        };

        SendMessageToAll(message);
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


        //send all the other player data to the new player 
        foreach (NetworkConnection conn in connections)
        {
            if (conn == newPlayerConnection) return;

            PlayerData otherPlayerData = GetPlayerData(conn);
            //if (otherPlayerData.name == "") return; //this caused bug that it doesnt add the other players who joined but didnt added a name

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
        //Debug.Log("Server StayAliveSend");
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
