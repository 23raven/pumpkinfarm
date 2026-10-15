using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlantSeedCondition : TutorialCondition
{
    [Header("Farm References")]
    [SerializeField] private FarmCropSystem farmCropSystem;
    [SerializeField] private Tilemap farmTilemap;

    private readonly HashSet<Vector3Int> initialPlantedCells =
        new HashSet<Vector3Int>();

    private bool initialized;

    public override bool IsMet(TutorialStep step)
    {
        if (farmCropSystem == null || farmTilemap == null)
            return false;

        // Запоминаем культуры, которые уже росли
        // к моменту начала проверки.
        if (!initialized)
        {
            foreach (Vector3Int cell in
                     farmTilemap.cellBounds.allPositionsWithin)
            {
                if (farmCropSystem.HasCropAt(cell))
                    initialPlantedCells.Add(cell);
            }

            initialized = true;
            return false;
        }

        // Проверяем, появилась ли новая культура.
        foreach (Vector3Int cell in
                 farmTilemap.cellBounds.allPositionsWithin)
        {
            if (farmCropSystem.HasCropAt(cell) &&
                !initialPlantedCells.Contains(cell))
            {
                return true;
            }
        }

        return false;
    }
}