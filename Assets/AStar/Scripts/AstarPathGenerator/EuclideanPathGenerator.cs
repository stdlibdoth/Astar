using System.Collections;
using System.Collections.Generic;
using AStar;
using UnityEngine;

public class EuclideanPathGenerator : PathGenerator
{
    public override AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target, MoveAgent agent)
    {
        return new EuclideanPath(grid as EuclideanGrid, start, target, agent);
    }
}
