using System.Collections;
using System.Collections.Generic;
using AStar;
using UnityEngine;

namespace AStar
{
    public class DiagonalEuclideanPathGenerator : PathGenerator
    {
        public override AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target, MoveAgent agent)
        {
            return new DiagonalEuclideanPath(grid as EuclideanGrid, start, target, agent);
        }
    }
}
