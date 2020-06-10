using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Room : MonoBehaviour
{

    public RoomData roomData = new RoomData();

    //ui
    [Space]
    [Header("Gameobject elements")]
    [SerializeField]
    private GameObject monster;
    [SerializeField]
    private GameObject exit;
    [SerializeField]
    private GameObject treasure;
    [SerializeField]
    private ButtonObject[] doors;
    [SerializeField]
    private GameObject[] otherPlayerObjects;
    [SerializeField]
    private GameObject myPlayerObj;

    [SerializeField]
    private GameObject buttonParent;
    [SerializeField]
    private Text turnText;

    private List<PlayerData> otherPlayersInRoom;
    public DataHolder dataHolder;

    void Start()
    {
        otherPlayersInRoom = new List<PlayerData>();
    }

    public void UpdateInfo(RoomInfoMessage message)
    {

        roomData.treasureAmmount = message.TreasureRoom;
        roomData.containsMonster = message.ContainsMonster == 1;
        roomData.containsExit = message.ContainsExit == 1;
        roomData.numberOfOtherPlayers = message.NumberOfOtherPlayers;
        roomData.otherPlayersIDs = message.OtherPlayerIDs;
        roomData.directions = roomData.ReadDirectionByte(message.MoveDirections);
        UpdateRoom(); 

    }

    private void UpdateRoom()
    {
        //update room
        doors[0].obj.SetActive(roomData.directions.North);
        doors[1].obj.SetActive(roomData.directions.East);
        doors[2].obj.SetActive(roomData.directions.South);
        doors[3].obj.SetActive(roomData.directions.West);

        monster.SetActive(roomData.containsMonster);
        exit.SetActive(roomData.containsExit);

        treasure.SetActive(roomData.treasureAmmount > 0);


        //update the player count
        if (otherPlayersInRoom == null) { otherPlayersInRoom = new List<PlayerData>(); }
        otherPlayersInRoom.Clear();
        UpdatePlayerUI();

        Debug.Log("otherd ids count" + roomData.otherPlayersIDs.Count);
        foreach (int id in roomData.otherPlayersIDs)
        {
            PlayerJoinedRoom(dataHolder.players.Find(x => x.playerIndex == id));
        } 
    }

    public void UpdateUI(bool myTurn, int playerID)
    {
        buttonParent.SetActive(myTurn);

        foreach(ButtonObject door in doors)
        {
            door.SetButtons(myTurn);
        }

        turnText.text = myTurn ? "Your turn!" : "Player #" + playerID + "'s turn...";
    }

    public void PlayerJoinedRoom(PlayerData data)
    {
        if (otherPlayersInRoom.Contains(data)) { return; }
        otherPlayersInRoom.Add(data);

        UpdatePlayerUI();
    }
    public void PlayerLeftRoom(PlayerData data)
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
}

[System.Serializable]
public class ButtonObject {
    public GameObject obj;
    public Button[] buttons;

    public void SetButtons(bool val)
    {
        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(val && obj.gameObject.activeSelf);
        }
    }
}
