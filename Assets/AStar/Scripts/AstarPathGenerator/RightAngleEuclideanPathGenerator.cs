using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class RightAngleEuclideanPathGenerator : PathGenerator
{
    public override AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target, MoveAgent agent)
    {
        return new RightAngleEuclideanPath(grid as EuclideanGrid, start, target, agent);
    }
}
