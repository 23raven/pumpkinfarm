using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaterCropsCondition : TutorialCondition
{
    [Header("Farm References")]
    [SerializeField] private FarmCropSystem farmCropSystem;
    [SerializeField] private Tilemap farmTilemap;

    private readonly List<Vector3Int> targetCells = new List<Vector3Int>();
    private bool targetsCaptured;

    public override bool IsMet(TutorialStep step)
    {
        if (farmCropSystem == null || farmTilemap == null)
            return false;

        // Wait until at least one living crop exists, then remember
        // the crops this step is asking the player to water.
        if (!targetsCaptured)
        {
            CaptureTargetCrops();

            if (targetCells.Count == 0)
                return false;

            targetsCaptured = true;
        }

        foreach (Vector3Int cell in targetCells)
        {
            if (!farmCropSystem.IsWateredToday(cell))
                return false;
        }

        return true;
    }

    private void CaptureTargetCrops()
    {
        targetCells.Clear();

        foreach (Vector3Int cell in farmTilemap.cellBounds.allPositionsWithin)
        {
            if (farmCropSystem.HasCropAt(cell) &&
                !farmCropSystem.IsWithered(cell))
            {
                targetCells.Add(cell);
            }
        }
    }
}
