using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;
[CreateAssetMenu(fileName = "aStarSettings", menuName = "ScriptableObjects/AStarSettings", order = 1)]
public class AStarSettings : ScriptableObject
{
    [SerializeField] public List<AStarLayer> layers;
}
