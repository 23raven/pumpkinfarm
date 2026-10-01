using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int day;
    public PlantRow[] plants = new PlantRow[6]; //квадратный массив сделал чтобы можно было обращаться отдельно к каждой клетке

    [SerializeField] private TMP_Text DayText;

    public void NextDay()
    {
        day++;
        DayText.text = "Day: " + day.ToString();
        GrowPlants();
    }

    public void GrowPlants()
    {
        foreach (PlantRow plantRow in plants)
        {
            foreach (Plant plant in plantRow.row)
            {
                if (plant.planted)
                {
                    plant.Grow();
                }
            }
        }
    }
}

[System.Serializable]
public class PlantRow
{
    public Plant[] row = new Plant[6];
}
