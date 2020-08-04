using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class HexagonalPathGenerator : PathGenerator
{
    public override AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target)
    {
        return new HexagonalPath(grid as HexagonalGrid, start, target);
    }

}
