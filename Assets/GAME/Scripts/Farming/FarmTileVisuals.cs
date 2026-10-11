using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public sealed class FarmTileVisuals : MonoBehaviour
{
    private Tilemap tilemap;
    private Tilemap waterOverlayTilemap;
    private TileBase tilledTile;
    private Color wateredTint = new Color(0.65f, 0.65f, 0.65f, 1f);

    private readonly Dictionary<Sprite, Tile> runtimeTiles =
        new Dictionary<Sprite, Tile>();

    private readonly HashSet<Vector3Int> wateredCells =
        new HashSet<Vector3Int>();

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        FindWaterOverlay();
    }

    public void Initialize(Tilemap targetTilemap, TileBase soilTile, Color wetTint)
    {
        tilemap = targetTilemap != null ? targetTilemap : GetComponent<Tilemap>();
        tilledTile = soilTile;
        wateredTint = wetTint;
        FindWaterOverlay();
    }

    private void FindWaterOverlay()
    {
        if (tilemap == null)
            return;

        Transform gridRoot = tilemap.transform.parent != null
            ? tilemap.transform.parent
            : tilemap.transform;

        Transform overlayTransform = gridRoot.Find("WaterOverlayTilemap");
        waterOverlayTilemap = overlayTransform != null
            ? overlayTransform.GetComponent<Tilemap>()
            : null;

        if (waterOverlayTilemap == null)
            return;

        TilemapRenderer mainRenderer = tilemap.GetComponent<TilemapRenderer>();
        TilemapRenderer overlayRenderer =
            waterOverlayTilemap.GetComponent<TilemapRenderer>();

        if (mainRenderer != null && overlayRenderer != null)
        {
            overlayRenderer.sortingLayerID = mainRenderer.sortingLayerID;
            overlayRenderer.sortingOrder = mainRenderer.sortingOrder + 1;
        }
    }

    public bool HasTile(Vector3Int cell)
    {
        return tilemap != null && tilemap.HasTile(cell);
    }

    public bool SetTilled(Vector3Int cell)
    {
        if (tilemap == null || tilledTile == null)
            return false;

        ResetColor(cell);
        tilemap.SetTile(cell, tilledTile);
        tilemap.RefreshTile(cell);
        return true;
    }

    public bool SetTile(Vector3Int cell, TileBase tile)
    {
        if (tilemap == null || tile == null)
            return false;

        ResetColor(cell);
        tilemap.SetTile(cell, tile);
        tilemap.RefreshTile(cell);
        return true;
    }

    public bool SetSprite(Vector3Int cell, Sprite sprite)
    {
        Tile tile = GetRuntimeTile(sprite);
        return tile != null && SetTile(cell, tile);
    }

    public void ShowWatered(Vector3Int cell)
    {
        if (tilemap == null)
            return;

        if (waterOverlayTilemap == null)
        {
            Debug.LogWarning(
                "FarmTileVisuals: Create a sibling Tilemap named 'WaterOverlayTilemap' under the same Grid.",
                this);
            return;
        }

        Sprite sprite = tilemap.GetSprite(cell);
        if (sprite == null)
        {
            Debug.LogWarning("FarmTileVisuals: No sprite found for watered cell.", this);
            return;
        }

        Tile overlayTile = GetOverlayTile(sprite);
        if (overlayTile == null)
            return;

        waterOverlayTilemap.SetTile(cell, overlayTile);
        waterOverlayTilemap.SetTileFlags(cell, TileFlags.None);
        waterOverlayTilemap.RefreshTile(cell);
        wateredCells.Add(cell);
    }

    public void ResetColor(Vector3Int cell)
    {
        if (tilemap != null && tilemap.HasTile(cell))
        {
            TileFlags flags = tilemap.GetTileFlags(cell);
            tilemap.SetTileFlags(cell, flags & ~TileFlags.LockColor);
            tilemap.SetColor(cell, Color.white);
            tilemap.RefreshTile(cell);
        }

        if (waterOverlayTilemap != null)
        {
            waterOverlayTilemap.SetTile(cell, null);
            waterOverlayTilemap.RefreshTile(cell);
        }

        wateredCells.Remove(cell);
    }

    private Tile GetRuntimeTile(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (runtimeTiles.TryGetValue(sprite, out Tile existing))
            return existing;

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = $"RuntimeTile_{sprite.name}";
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        tile.flags = TileFlags.None;

        runtimeTiles.Add(sprite, tile);
        return tile;
    }

    private Tile GetOverlayTile(Sprite sprite)
    {
        if (sprite == null)
            return null;

        if (overlayTiles.TryGetValue(sprite, out Tile existingOverlay))
            return existingOverlay;

        float brightness = (wateredTint.r + wateredTint.g + wateredTint.b) / 3f;
        float darknessAlpha = Mathf.Clamp01(1f - brightness);

        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = $"WaterOverlay_{sprite.name}";
        tile.sprite = sprite;
        tile.color = new Color(0f, 0f, 0f, darknessAlpha);
        tile.colliderType = Tile.ColliderType.None;
        tile.flags = TileFlags.None;

        // Use a dedicated cache key path to avoid mixing normal and overlay tiles.
        overlayTiles[sprite] = tile;
        return tile;
    }

    private readonly Dictionary<Sprite, Tile> overlayTiles =
        new Dictionary<Sprite, Tile>();

    private void OnDestroy()
    {
        if (waterOverlayTilemap != null)
            waterOverlayTilemap.ClearAllTiles();

        foreach (Tile tile in runtimeTiles.Values)
        {
            if (tile != null)
                Destroy(tile);
        }

        foreach (Tile tile in overlayTiles.Values)
        {
            if (tile != null)
                Destroy(tile);
        }

        runtimeTiles.Clear();
        overlayTiles.Clear();
        wateredCells.Clear();
    }
}
