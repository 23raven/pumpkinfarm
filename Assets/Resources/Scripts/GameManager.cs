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

        CheckAndMergePumpkins();
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

    //честно сам не очень понимаю как я это сделал просто проходимся по каждой клетке и проверяем ее
    public void CheckAndMergePumpkins()
    {
        for (int y = 0; y < plants.Length - 1; y++)
        {
            for (int x = 0; x < plants[y].row.Length - 1; x++)
            {
                //получаем клетки
                Plant topLeft = plants[y].row[x];
                Plant topRight = plants[y + 1].row[x];
                Plant bottomLeft = plants[y].row[x + 1];
                Plant bottomRight = plants[y + 1].row[x + 1];

                if (CanMerge(topLeft) && CanMerge(topRight) && CanMerge(bottomLeft) && CanMerge(bottomRight))
                {
                    MergeIntoBigPumpkin(topLeft, topRight, bottomLeft, bottomRight);
                }
            }
        }
    }

    //вспомогательный метод для проверки
    private bool CanMerge(Plant plant)
    {
        if (plant == null) return false;

        return plant.planted && !plant.dead && plant.stage == 6;
    }

    //вынес метод для обьеденения тыкв может понадобится для расширения механики
    private void MergeIntoBigPumpkin(Plant p1, Plant p2, Plant p3, Plant p4)
    {
        p1.ChangeForBig();
        p2.PickUp();
        p3.PickUp();
        p4.PickUp();
    }
}

[System.Serializable]
public class PlantRow
{
    public Plant[] row = new Plant[6];
}
