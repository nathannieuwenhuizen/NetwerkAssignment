using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerDataHolder : MonoBehaviour
{

    public List<PlayerData> players;
    public List<int> activePlayerIDs;
    public List<int[]> activeMonsters;

    public RoomData[,] rooms;
    public int[] startIndex;
    public int roomSize = 3;

    public int turnID = 0;
    void Start()
    {
        players = new List<PlayerData>();
    }

    private void CreateRoomData()
    {
        rooms = new RoomData[roomSize, roomSize];

        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                RoomData newRoom = new RoomData()
                {
                    directions = new Directions //set boundaries to rooms
                    {
                        North = y > 0,
                        East = x < roomSize - 1,
                        South = y < roomSize - 1,
                        West = x > 0
                    }
                };
                if (x == 1 && y == 0)
                {
                    newRoom.containsMonster = true;
                }
                if (x == 2 && y == 0)
                {
                    newRoom.treasureAmmount = 100;
                }
                rooms[x, y] = newRoom;
            }
        }
    }

    //gets the room that is connected to the previous room if its not out of boundaries and has a door connected to it.
    public int[] GetNextRoomID(RoomData currentRoom, byte dirByte)
    {
        int[] result = null;
        int[] currentIndex = Tools.FindIndex(rooms, currentRoom); //needs testing!
        int xPos = currentIndex[0];
        int yPos = currentIndex[1];
        //Debug.Log("currentRoom index: "+ currentIndex[0] + " | " + currentIndex[1]);
        DirectionEnum directionEnum = (DirectionEnum)dirByte;
        //Debug.Log("direction enum: " + directionEnum);
        switch (directionEnum)
        {
            case DirectionEnum.North:
                if (currentRoom.directions.North && yPos >  0)
                {
                    result = new int[]{ xPos, yPos - 1};
                }
                break;
            case DirectionEnum.East: 
                if (currentRoom.directions.East && xPos < roomSize - 1)
                {
                    result = new int[] { xPos + 1, yPos };
                }
                break;
            case DirectionEnum.South:
                if (currentRoom.directions.South && yPos < roomSize - 1)
                { 
                    result = new int[] { xPos, yPos + 1 };
                }
                break;
            case DirectionEnum.West:
                if (currentRoom.directions.West && xPos > 0)
                {
                    result = new int[] { xPos - 1, yPos };
                }
                break;
            default:
                break;
        }
        if (result != null)
        {
            //Debug.Log("newRoom index: " + result[0] + " | " + result[1]);
        }

        return result;
    }


    public void GameSetup()
    {
        CreateRoomData();

        startIndex = new int[] { 0, 0 };
        activePlayerIDs = new List<int>();
        activeMonsters = new List<int[]>();
        //set all players data room to start room. and add their ids to the active dungeon ids list
        foreach(PlayerData player in players)
        {
            player.roomID = startIndex;
            activePlayerIDs.Add(player.playerIndex);
        }


    } 

    public RoomInfoMessage GetRoomMessage(int playerID)
    {
        PlayerData data = players.Find(x => x.playerIndex == playerID); //get player data

        //get room data
        int[] roomIndex = data.roomID;
        Debug.Log("room index: [" + roomIndex[0] + " , " + roomIndex[1] + " ]");
        RoomData room = rooms[roomIndex[0], roomIndex[1]];

        List<int> otherPlayerIDs = GetOtherPlayerIDsInSameRoom(data);
        Debug.Log("amount of other players ids in that room" + otherPlayerIDs.Count);

        if (room.containsMonster)
        {
            if (activeMonsters.Contains(data.roomID))
            {
                activeMonsters.Add(data.roomID);
            }
        }

        return new RoomInfoMessage()
        {
            MoveDirections = room.GetDirsByte(),
            TreasureRoom = (ushort)room.treasureAmmount,
            ContainsMonster = (byte)(room.containsMonster ? 1 : 0),
            ContainsExit = (byte)(room.containsExit ? 1 : 0),
            NumberOfOtherPlayers = (byte)otherPlayerIDs.Count,
            OtherPlayerIDs = otherPlayerIDs
        }; 

    }

    public List<int> GetOtherPlayerIDsInSameRoom(PlayerData data)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].roomID[0] == data.roomID[0] && players[i].roomID[1] == data.roomID[1] && players[i].playerIndex != data.playerIndex && players[i].activeInDungeon && players[i].hp > 0)
            {
                result.Add(players[i].playerIndex);
            } 
        }
        return result;
    } 

    public List<int> GetPlayerIDsRoom(RoomData data)
    {
        List<int> result = new List<int>();
        //Debug.Log("room id" + Tools.FindIndex(rooms, data));
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].roomID[0] == Tools.FindIndex(rooms, data)[0] && players[i].roomID[1] == Tools.FindIndex(rooms, data)[1])
            {
                result.Add(players[i].playerIndex);
                //Debug.Log("players in current room" + players[i].playerIndex);
            }
        }
        return result;
    }

}
