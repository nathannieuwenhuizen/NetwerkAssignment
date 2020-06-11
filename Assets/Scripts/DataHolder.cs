using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataHolder : MonoBehaviour
{

    public List<PlayerData> players;
    public PlayerData myData;

    public Lobby lobby;
    public Game game;
    public ClientBehaviour client;

    void Start()
    {
        myData = new PlayerData();
        players = new List<PlayerData>();
    }

    public void StartGame()
    {
        //SceneManager.LoadScene("Game");
        lobby.gameObject.SetActive(false);
        game = Instantiate(Resources.Load<GameObject>("Game") as GameObject).GetComponent<Game>();
        game.gameObject.name = "Game of player: " + myData.name;
        game.cRoom.dataHolder = this;
        game.dataHolder = this;
        game.SetupGame();
    }


}
[System.Serializable]
public class PlayerData
{
    public int playerIndex;
    public string name;
    public Color color;
    public int hp = 0;
    public int[] roomID; // 2d array index [0,0]
    public int score = 0;
}