using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHolder : MonoBehaviour
{

    public List<PlayerData> players;
    public PlayerData myData;

    public Lobby lobby;

    void Start()
    {
        myData = new PlayerData();
        players = new List<PlayerData>();
    }

}
[System.Serializable]
public class PlayerData
{
    public int playerIndex;
    public string name;
    public Color color;
}