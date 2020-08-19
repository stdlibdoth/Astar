using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    [CreateAssetMenu(fileName = "obstacleSettings", menuName = "AStarScriptableObjs/AgentObstacleSettings", order = 1)]
    public class AgentObstacleSetting : ScriptableObject
    {
        [Header("Astar Layer of this agent")]
        [SerializeField] public AStarLayer astarLayer;
        [Header("Obstacle Astar Layers")]
        [SerializeField] public List<AStarLayer> obstacleAstarLayers;
    }
}
