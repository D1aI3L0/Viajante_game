using System;
using UnityEngine;


[Serializable]
public class Upgrade
{
    public Vector2Int gridPosition;

    protected static Vector2Int[] oddPositionsMove =
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1)
    };

    protected static Vector2Int[] evenPositionsMove =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(-1, 1)
    };

    public Vector2Int[] GetHexNeighbors()
    {
        if ((gridPosition.y % 2) != 0)
        {
            return new Vector2Int[]
            {
                gridPosition + oddPositionsMove[0],
                gridPosition + oddPositionsMove[1],
                gridPosition + oddPositionsMove[2],
                gridPosition + oddPositionsMove[3],
                gridPosition + oddPositionsMove[4],
                gridPosition + oddPositionsMove[5]
            };
        }
        return new Vector2Int[]
        {
            gridPosition + evenPositionsMove[0],
            gridPosition + evenPositionsMove[1],
            gridPosition + evenPositionsMove[2],
            gridPosition + evenPositionsMove[3],
            gridPosition + evenPositionsMove[4],
            gridPosition + evenPositionsMove[5]
        };
    }

    public Vector2Int CalculatePosition( HexDirection direction)
    {
        if((gridPosition.y % 2) != 0)
            return gridPosition + oddPositionsMove[(int)direction];

        return gridPosition + evenPositionsMove[(int)direction];
    }
}