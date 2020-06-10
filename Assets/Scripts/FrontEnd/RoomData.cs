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

public class RoomData
{
    //message
    public Directions directions = new Directions();
    public int treasureAmmount = 0;
    public bool containsMonster = false;
    public bool containsExit = false;
    public int numberOfOtherPlayers = 0;
    public List<int> otherPlayersIDs = new List<int>();

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

    public byte GetByteFromDirection(DirectionEnum Enum)
    {
        byte answer = 0;
        answer |= (byte)Enum;
        return answer;
    }
}
