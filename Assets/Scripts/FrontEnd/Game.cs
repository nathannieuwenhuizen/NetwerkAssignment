using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//game from the client side
public class Game : MonoBehaviour
{
    public Room cRoom;
    public int currentTurnID;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void SetupGame()
    {
        Debug.Log("Game is setting up");
    }


    public void UpdateRoom(RoomInfoMessage message)
    {
        cRoom.UpdateInfo(message);
    }



    public void PlayerTurn(int turnID, int myID)
    {
        currentTurnID = turnID;
        if (currentTurnID == myID)
        {

        } else
        {

        }
    }

}
