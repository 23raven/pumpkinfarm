using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public int day;
    public PlantRow[] plants = new PlantRow[6]; //квадратный массив сделал чтобы можно было обращаться отдельно к каждой клетке
    public int money;

    [SerializeField] private TMP_Text DayText;
    [SerializeField] private TMP_Text MoneyText;

    public void NextDay()
    {
        day++;
        DayText.text = "Day: " + day.ToString();
        GrowPlants();
    }

    public void DrawMoneyText()
    {
        MoneyText.text = "money: " + money.ToString();
    }

    private void Start()
    {
        DrawMoneyText();
    }

    public void AddMoney(int count)
    {
        money += count;
        DrawMoneyText();
    }

    public void RemoveMoney(int count)
    {
        money -= count;
        DrawMoneyText();
    }

    public void GrowPlants()
    {
        foreach (PlantRow plantRow in plants)
        {
            foreach (Plant plant in plantRow.row)
            {
                if (plant.planted)
                {
                    if (plant.watered && !plant.dead)
                    {
                        plant.Grow();
                    }
                    else
                    {
                        plant.Death();
                    }
                }

                plant.watered = false;
                plant.transform.parent.gameObject.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
            }
        }
    }
}

[System.Serializable]
public class PlantRow
{
    public Plant[] row = new Plant[6];
}
