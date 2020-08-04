using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public abstract class PathGenerator:MonoBehaviour
{
    public abstract AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target) where T : AStarGrid;
}