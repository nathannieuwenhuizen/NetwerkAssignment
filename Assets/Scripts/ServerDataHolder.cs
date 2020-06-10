using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerDataHolder : MonoBehaviour
{

    public List<PlayerData> players; 
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
                        East = x < roomSize,
                        South = y < roomSize,
                        West = x > 0
                    }
                };
                rooms[x, y] = newRoom;
            }
        }
    }

    //gets the room that is connected to the previous room if its not out of boundaries and has a door connected to it.
    public int[] GetNextRoomID(RoomData previousRoom, byte dirByte)
    {
        int[] result = null;
        int[] index = Tools.FindIndex(rooms, result); //needs testing!
        int xPos = index[0];
        int yPos = index[0];
        Debug.Log("currentRoom index: "+ index.Length);
        DirectionEnum directionEnum = (DirectionEnum)dirByte;
        Debug.Log("direction enum: " + directionEnum);
        switch (directionEnum)
        {
            case DirectionEnum.North:
                if (previousRoom.directions.North && yPos >  0)
                {
                    result = new int[]{ xPos, yPos - 1};
                }
                break;
            case DirectionEnum.East:
                if (previousRoom.directions.East && xPos < roomSize - 1)
                {
                    result = new int[] { xPos + 1, yPos };
                }
                break;
            case DirectionEnum.South:
                if (previousRoom.directions.South && yPos < roomSize - 1)
                {
                    result = new int[] { xPos, yPos + 1 };
                }
                break;
            case DirectionEnum.West:
                if (previousRoom.directions.East && xPos > 0)
                {
                    result = new int[] { xPos - 1, yPos };
                }
                break;
            default:
                break;
        }
        return result;
    }


    public void GameSetup()
    {
        CreateRoomData();

        startIndex = new int[] { 0, 0 };
        //set all players data room to start room.
        foreach(PlayerData player in players)
        {
            player.roomID = startIndex;
        }
    } 

    public RoomInfoMessage GetRoomMessage(int playerID)
    {
        PlayerData data = players.Find(x => x.playerIndex == playerID); //get player data

        //get room data
        int[] roomIndex = data.roomID;
        RoomData room = rooms[roomIndex[0], roomIndex[1]];

        List<int> otherPlayerIDs = GetOtherPlayerIDsInSameRoom(data);
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
            if (players[i].roomID == data.roomID && players[i] != data)
            {
                result.Add(players[i].playerIndex);
            }
        }
        return result;
    }

    public List<int> GetPlayerIDsRoom(RoomData data)
    {
        List<int> result = new List<int>();

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].roomID == Tools.FindIndex(rooms, data))
            {
                result.Add(players[i].playerIndex);
            }
        }
        return result;
    }

}
