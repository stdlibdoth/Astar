using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AStar;

public class HexagonalGrid : AStarGrid
{
    [Header("2D Hexagonal tile radius")]
    [SerializeField] private float m_tileRadius = 2;
    public override Vector2 TileSize { get { return new Vector2(2*m_tileRadius,2*m_tileRadius); } }
    public override Vector2 GridHBound
    {
        get
        {
            return new Vector2((hSize.x * 2 + Mathf.Clamp(hSize.y, 0, 1)) * Mathf.Cos(30 * Mathf.Deg2Rad) * m_tileRadius,
                (2 * m_tileRadius) + ((hSize.y - 1) * 1.5f * m_tileRadius));
        }
    }

    public List<AStarTile> GetAdjacentTiles(AStarTile tile)
    {
        List<AStarTile> tiles = new List<AStarTile>();
        if (tile.Grid != this)
            return tiles;
        if (Mathf.Abs(tile.Y % 2) == 0)
        {
            if (CheckBoundary(tile.X - 1, tile.Y + 1))
                tiles.Add(GetTile(tile.X - 1, tile.Y + 1));
            if (CheckBoundary(tile.X - 1, tile.Y - 1))
                tiles.Add(GetTile(tile.X - 1, tile.Y - 1));
        }
        else
        {
            if (CheckBoundary(tile.X + 1, tile.Y + 1))
                tiles.Add(GetTile(tile.X + 1, tile.Y + 1));
            if (CheckBoundary(tile.X + 1, tile.Y - 1))
                tiles.Add(GetTile(tile.X + 1, tile.Y - 1));
        }

        if (CheckBoundary(tile.X, tile.Y + 1))
            tiles.Add(GetTile(tile.X, tile.Y + 1));
        if (CheckBoundary(tile.X, tile.Y - 1))
            tiles.Add(GetTile(tile.X, tile.Y - 1));
        if (CheckBoundary(tile.X + 1, tile.Y))
            tiles.Add(GetTile(tile.X + 1, tile.Y));
        if (CheckBoundary(tile.X - 1, tile.Y))
            tiles.Add(GetTile(tile.X - 1, tile.Y));

        return tiles;
    }

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
                float xpos = 0;
                if (Mathf.Abs(i % 2) == 1)
                    xpos = (j + 0.5f) * TileSize.x * Mathf.Cos(30 * Mathf.Deg2Rad);
                else
                    xpos = j * TileSize.x * Mathf.Cos(30 * Mathf.Deg2Rad);
                t.transform.localPosition = new Vector3(xpos, 0, i * 0.5f * TileSize.y * 1.5f);
                m_tiles[j + hSize.x, i + hSize.y] = t;
            }
        }

        OnInit.Invoke();
        OnInit.RemoveAllListeners();
        yield return null;
    }
}