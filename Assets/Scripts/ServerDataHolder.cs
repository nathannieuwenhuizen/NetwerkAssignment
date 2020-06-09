using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerDataHolder : MonoBehaviour
{

    public List<PlayerData> players;
    public Room[,] rooms;
    public int[] startIndex;
    public int roomSize = 3;

    void Start()
    {
        players = new List<PlayerData>();
    }

    private void CreateRoomData()
    {
        rooms = new Room[roomSize, roomSize];

        for (int x = 0; x < roomSize; x++)
        {
            for (int y = 0; y < roomSize; y++)
            {
                Room newRoom = new Room()
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
        Room room = rooms[roomIndex[0], roomIndex[1]];

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


    //public  int[] FindIndex(this Array haystack, object needle)
    //{
    //    if (haystack.Rank == 1)
    //        return new[] { Array.IndexOf(haystack, needle) };

    //    var found = haystack.OfType<object>()
    //                      .Select((v, i) => new { v, i })
    //                      .FirstOrDefault(s => s.v.Equals(needle));
    //    if (found == null)
    //        throw new Exception("needle not found in set");

    //    var indexes = new int[haystack.Rank]; 
    //    var last = found.i;
    //    var lastLength = Enumerable.Range(0, haystack.Rank)
    //                               .Aggregate(1,
    //                                   (a, v) => a * haystack.GetLength(v));
    //    for (var rank = 0; rank < haystack.Rank; rank++)
    //    {
    //        lastLength = lastLength / haystack.GetLength(rank);
    //        var value = last / lastLength;
    //        last -= value * lastLength;

    //        var index = value + haystack.GetLowerBound(rank);
    //        if (index > haystack.GetUpperBound(rank))
    //            throw new IndexOutOfRangeException();
    //        indexes[rank] = index;
    //    }

    //    return indexes;
    //}

}
