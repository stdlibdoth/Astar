using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class EuclideanGrid : AStarGrid
{
    [Header("2D Euclidean tile size")]
    [SerializeField] private Vector2 m_tileSize = new Vector2(0,0);
    public override Vector2 TileSize { get { return m_tileSize; } }
    public override Vector2 GridHBound { get { return new Vector2(hSize.x * TileSize.x, hSize.y * TileSize.y); } }

    protected override IEnumerator Generate()
    {
        if (m_routesHolder != null)
            Destroy(m_routesHolder.gameObject);
        m_routesHolder = new GameObject("Routes").transform;
        m_routesHolder.SetParent(transform);

        if (m_tilesHolder != null)
            Destroy(m_tilesHolder.gameObject);
        m_tilesHolder = new GameObject("Tiles Holder").transform;
        m_tilesHolder.SetParent(transform);


        m_tiles = new AStarTile[2 * hSize.x, 2 * hSize.y];
        for (int i = -hSize.y; i < hSize.y; i++)
        {
            for (int j = -hSize.x; j < hSize.x; j++)
            {
                AStarTile t = Instantiate<AStarTile>(m_tilePrefab, m_tilesHolder).InitTile(this, j, i);
                t.transform.localPosition = new Vector3(j * TileSize.x, 0, i * TileSize.y);
                m_tiles[j + hSize.x, i + hSize.y] = t;
            }
        }

        OnInit.Invoke();
        OnInit.RemoveAllListeners();
        yield return null;
    }

    public override List<AStarTile> GetAdjacentTiles(AStarTile tile)
    {
        List<AStarTile> tiles = new List<AStarTile>();
        if (tile.Grid != this)
            return tiles;

        if (CheckBoundary(tile.X, tile.Y + 1))
            tiles.Add(GetTile(tile.X, tile.Y + 1));
        if (CheckBoundary(tile.X + 1, tile.Y + 1))
            tiles.Add(GetTile(tile.X + 1, tile.Y + 1));
        if (CheckBoundary(tile.X + 1, tile.Y))
            tiles.Add(GetTile(tile.X + 1, tile.Y));
        if (CheckBoundary(tile.X + 1, tile.Y - 1))
            tiles.Add(GetTile(tile.X + 1, tile.Y - 1));
        if (CheckBoundary(tile.X, tile.Y - 1))
            tiles.Add(GetTile(tile.X, tile.Y - 1));
        if (CheckBoundary(tile.X - 1, tile.Y - 1))
            tiles.Add(GetTile(tile.X - 1, tile.Y - 1));
        if (CheckBoundary(tile.X - 1, tile.Y))
            tiles.Add(GetTile(tile.X - 1, tile.Y));
        if (CheckBoundary(tile.X - 1, tile.Y + 1))
            tiles.Add(GetTile(tile.X - 1, tile.Y + 1));
        return tiles;
    }
}
