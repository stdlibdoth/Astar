using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    public abstract class PathGenerator : MonoBehaviour
    {
        public abstract AStarPath GeneratePath<T>(T grid, AStarTile start, AStarTile target, MoveAgent agent) where T : AStarGrid;
    }
}