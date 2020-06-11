using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Tools
{

    public static int[] FindIndex( RoomData[,] array, RoomData obj)
    {
        //Debug.Log("Array length" + );
        for (int x = 0; x <array.GetLength(0); x++)
        {
            for (int y = 0; y < array.GetLength(1); y++)
            {
                if (array[x,y] != null)
                {
                    if (array[x, y] == obj)
                    {
                        return new int[] { x, y };
                    }
                }
            }
        }
        return null;
    }
}