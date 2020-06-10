using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//game from the client side
public class Game : MonoBehaviour
{
    public Room cRoom;
    public DataHolder dataHolder;

    public int currentTurnID;

    public void SetupGame()
    {
        Debug.Log("Game is setting up");
    }


    public void UpdateRoom(RoomInfoMessage message)
    {
        cRoom.UpdateInfo(message);
    }

    public void GoToWest()
    {
        dataHolder.client.SendMoveRequest(DirectionEnum.West);
    }

    public void GoToNorth()
    {
        dataHolder.client.SendMoveRequest(DirectionEnum.North);
    }

    public void GoToSouth()
    {
        dataHolder.client.SendMoveRequest(DirectionEnum.South);
    }

    public void GoToEast()
    {
        dataHolder.client.SendMoveRequest(DirectionEnum.East);
    }

    public void PlayerTurn(int turnID)
    {
        currentTurnID = turnID;
        if (currentTurnID == dataHolder.myData.playerIndex)
        {
            cRoom.UpdateUI(true, currentTurnID);
        } else
        {
            cRoom.UpdateUI(false, currentTurnID);
        }
    }

}
