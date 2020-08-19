using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

namespace AStar
{
    [CreateAssetMenu(fileName = "aStarSettings", menuName = "AStarScriptableObjs/AStarSettings", order = 1)]
    public class AStarSettings : ScriptableObject
    {
        [SerializeField] public List<AStarLayer> layers;
    }
}
