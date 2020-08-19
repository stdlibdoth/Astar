using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    public class HexagonalPathGenerator : PathGenerator
    {
        public override AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target, MoveAgent agent)
        {
            return new HexagonalPath(grid as HexagonalGrid, start, target, agent);
        }

    }
}