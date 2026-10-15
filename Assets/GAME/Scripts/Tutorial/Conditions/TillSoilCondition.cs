using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TillSoilCondition : TutorialCondition
{
    [Header("Farm References")]
    [SerializeField] private Tilemap farmTilemap;
    [SerializeField] private TileBase tilledTile;

    private readonly HashSet<Vector3Int> initialTilledCells =
        new HashSet<Vector3Int>();

    private readonly HashSet<Vector3Int> newlyTilledCells =
        new HashSet<Vector3Int>();

    private bool initialized;

    public override bool IsMet(TutorialStep step)
    {
        if (farmTilemap == null || tilledTile == null)
            return false;

        // Запоминаем клетки, которые уже были вспаханы
        // к моменту начала проверки условия.
        if (!initialized)
        {
            foreach (Vector3Int cell in
                     farmTilemap.cellBounds.allPositionsWithin)
            {
                if (farmTilemap.GetTile(cell) == tilledTile)
                    initialTilledCells.Add(cell);
            }

            initialized = true;
            return false;
        }

        // Ищем новые вспаханные клетки.
        foreach (Vector3Int cell in
                 farmTilemap.cellBounds.allPositionsWithin)
        {
            if (farmTilemap.GetTile(cell) != tilledTile)
                continue;

            if (!initialTilledCells.Contains(cell))
                newlyTilledCells.Add(cell);
        }

        return newlyTilledCells.Count > 0;
    }
}