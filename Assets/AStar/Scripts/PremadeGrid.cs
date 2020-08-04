using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class PremadeGrid : AStarGrid
{
    public override Vector2 TileSize => throw new System.NotImplementedException();

    public override Vector2 GridHBound => throw new System.NotImplementedException();

    protected override IEnumerator Generate()
    {
        throw new System.NotImplementedException();
    }
}
