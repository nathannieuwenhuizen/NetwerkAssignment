using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Flags]
public enum DirectionEnum
{
    North = 1,
    East = 2,
    South = 4,
    West = 8
}

public class Directions
{
    public bool North;
    public bool East;
    public bool South;
    public bool West;
}

public class Room : MonoBehaviour
{

    public byte directionByte;

    public Directions directions = new Directions();
    public int treasureAmmount = 0;
    public bool containsMonster = false;
    public bool containsExit = false;
    public int numberOfOtherPlayers = 0;
    public List<int> otherPlayersIDs = new List<int>();

    [Space]
    [Header("UI elements")]
    [SerializeField]
    private GameObject monster;
    [SerializeField]
    private GameObject exit;
    [SerializeField]
    private GameObject treasure;
    [SerializeField]
    private GameObject[] doors;
    [SerializeField]
    private GameObject[] otherPlayerObjects;
     
    [SerializeField]
    private GameObject myPlayerObj;

    private List<PlayerData> otherPlayersInRoom;

    public DataHolder dataHolder;

    void Start()
    {
        otherPlayersInRoom = new List<PlayerData>();
    }

    public void UpdateInfo(RoomInfoMessage message)
    {
        treasureAmmount = message.TreasureRoom;
        containsMonster = message.ContainsMonster == 1;
        containsExit = message.ContainsExit == 1;
        numberOfOtherPlayers = message.NumberOfOtherPlayers;
        otherPlayersIDs = message.OtherPlayerIDs;
        directions = ReadDirectionByte(message.MoveDirections);
        UpdateRoom();

    }

    private void UpdateRoom()
    {
        //update room
        doors[0].SetActive(directions.North);
        doors[1].SetActive(directions.East);
        doors[2].SetActive(directions.South);
        doors[3].SetActive(directions.West);

        monster.SetActive(containsMonster);
        exit.SetActive(containsExit);

        treasure.SetActive(treasureAmmount > 0);


        //update the player count
        if (otherPlayersInRoom == null) { otherPlayersInRoom = new List<PlayerData>(); }
        otherPlayersInRoom.Clear();
        UpdatePlayerUI();
        Debug.Log("otherd ids count" + otherPlayersIDs.Count);
        foreach (int id in otherPlayersIDs)
        {
            PlayerJoined(dataHolder.players.Find(x => x.playerIndex == id));
        } 
    }

    public void PlayerJoined(PlayerData data)
    {
        if (otherPlayersInRoom.Contains(data)) { return; }
        otherPlayersInRoom.Add(data);

        UpdatePlayerUI();
    }
    public void PleayerLeave(PlayerData data)
    {
        if (!otherPlayersInRoom.Contains(data)) { return; }
        otherPlayersInRoom.Remove(data);

        UpdatePlayerUI();
    }

    private void UpdatePlayerUI()
    {
        myPlayerObj.SetActive(true);
        myPlayerObj.GetComponent<SpriteRenderer>().color = dataHolder.myData.color;

        //Debug.Log(otherPlayersInRoom.Count);
        for(int i = 0; i < otherPlayerObjects.Length; i++)
        {
            if (i < otherPlayersInRoom.Count) // needs testing
            {
                otherPlayerObjects[i].SetActive(true);
                otherPlayerObjects[i].GetComponent<SpriteRenderer>().color = otherPlayersInRoom[i].color;
            } 
            else
            {
                otherPlayerObjects[i].SetActive(false);
            }
        }
    }

    public Directions ReadDirectionByte(byte dirs)
    {
        return new Directions
        {
            North = (dirs & (byte)DirectionEnum.North) == (byte)DirectionEnum.North,
            East = (dirs & (byte)DirectionEnum.East) == (byte)DirectionEnum.East,
            South = (dirs & (byte)DirectionEnum.South) == (byte)DirectionEnum.South,
            West = (dirs & (byte)DirectionEnum.West) == (byte)DirectionEnum.West
        };
    }

    public byte GetDirsByte()
    {
        byte answer = 0;
        if (directions.North) answer |= (byte)DirectionEnum.North;
        if (directions.East) answer |= (byte)DirectionEnum.East;
        if (directions.South) answer |= (byte)DirectionEnum.South;
        if (directions.West) answer |= (byte)DirectionEnum.West;

        return answer;
    }
}
