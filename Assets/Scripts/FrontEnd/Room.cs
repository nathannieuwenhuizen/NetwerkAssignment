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
    private ButtonObject monster;
    [SerializeField]
    private ButtonObject exit;
    [SerializeField]
    public ButtonObject treasure;
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
    [SerializeField]
    private Text hp;
    [SerializeField]
    private Text score;


    private bool IminDungeon = true;

    public List<PlayerData> otherPlayersInRoom;
    public DataHolder dataHolder;

    void Start()
    {
        otherPlayersInRoom = new List<PlayerData>();
    }

    public void UpdateInfo(RoomInfoMessage message)
    {

        roomData.treasureAmmount = message.TreasureRoom;
        roomData.containsMonster = message.ContainsMonster == 1;
        if (roomData.containsMonster)
        {
            roomData.monsterHP = 10;
        }
        roomData.containsExit = message.ContainsExit == 1;
        roomData.numberOfOtherPlayers = message.NumberOfOtherPlayers;
        roomData.otherPlayersIDs = message.OtherPlayerIDs;
        roomData.directions = roomData.ReadDirectionByte(message.MoveDirections);
        UpdateRoom(); 

    } 

    public void UpdateRoom()
    {
        //update room
        doors[0].obj.SetActive(roomData.directions.North);
        doors[1].obj.SetActive(roomData.directions.East);
        doors[2].obj.SetActive(roomData.directions.South);
        doors[3].obj.SetActive(roomData.directions.West);

        monster.obj.SetActive(roomData.containsMonster);
        exit.obj.SetActive(roomData.containsExit);

        treasure.obj.SetActive(roomData.treasureAmmount > 0);


        //update the player count
        if (otherPlayersInRoom == null) { otherPlayersInRoom = new List<PlayerData>(); }
        otherPlayersInRoom.Clear();

        //Debug.Log("otherd ids count" + roomData.otherPlayersIDs.Count);
        foreach (int id in roomData.otherPlayersIDs)
        {
            PlayerJoinedRoom(dataHolder.players.Find(x => x.playerIndex == id));
        }
        UpdatePlayerUIElements();
    }

    public void UpdateHPText()
    {
        hp.text = "HP: " + dataHolder.myData.hp;
    }

    public void UpdateUI(bool myTurn, int playerID)
    {
        buttonParent.SetActive(myTurn);

        foreach(ButtonObject door in doors)
        {
            door.SetButtons(myTurn);
        }
        treasure.SetButtons(myTurn);
        exit.SetButtons(myTurn);
        monster.SetButtons(myTurn);

        hp.text = "HP: " + dataHolder.myData.hp;
        score.text = "Score: " + dataHolder.myData.score;

        string name = "";
        name = otherPlayersInRoom.Find(x => x.playerIndex == playerID)?.name;
        turnText.text = myTurn ? "Your turn!" : "#" + playerID + " " + name + "'s turn...";

        
    }
    private void Update()
    {
        //Debug.Log("other player count: " + otherPlayersInRoom.Count);
    }
    public void PlayerJoinedRoom(PlayerData data)
    {
        //Debug.Log("player enter room" + data);

        otherPlayersInRoom.Add(data);

    }
    public void PlayerLeftRoom(PlayerData data)
    {
        //Debug.Log("player left room" + data);

        otherPlayersInRoom.Remove(data);
    }

    public void ILeftDungeon()
    {
        IminDungeon = false;
    }

    public void UpdatePlayerUIElements()
    {
        myPlayerObj.SetActive(IminDungeon);
        myPlayerObj.GetComponentInChildren<TextMesh>().text = dataHolder.myData.name; 
        myPlayerObj.GetComponent<SpriteRenderer>().color = dataHolder.myData.color;



        for (int i = 0; i < otherPlayerObjects.Length; i++)
        {
            otherPlayerObjects[i].SetActive(false);
        }
        for (int i = 0; i < otherPlayersInRoom.Count; i++)
        {
            if (otherPlayersInRoom[i] != null)
            {
                //Debug.Log("player object: " + otherPlayerObjects[i]);
                //Debug.Log("player in room: " + otherPlayersInRoom[i]);
                otherPlayerObjects[i].SetActive(true);
                otherPlayerObjects[i].GetComponentInChildren<TextMesh>().text = otherPlayersInRoom[i].name;
                otherPlayerObjects[i].GetComponent<SpriteRenderer>().color = otherPlayersInRoom[i].color;
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
