using System;
using System.Collections.Generic;
using UnityEngine;

// Таблица рецептов резьбы.
// Каждая строка: "из этой тыквы" + "это изменение" = "вот такая тыква".
[CreateAssetMenu(
    fileName = "CarvingRecipeTable",
    menuName = "Game/Carving/Recipe Table"
)]
public class CarvingRecipeTable : ScriptableObject
{
    [Serializable]
    public class Recipe
    {
        public ItemDefinition source;
        public Ornament change;   // поле icon здесь можно не заполнять
        public ItemDefinition result;
    }

    [SerializeField] private List<Recipe> recipes = new List<Recipe>();

    // Можно ли эту тыкву резать (есть ли хоть один рецепт с ней)
    public bool IsCarvable(ItemDefinition item)
    {
        if (item == null) return false;

        foreach (Recipe recipe in recipes)
        {
            if (recipe != null && recipe.source == item)
                return true;
        }
        return false;
    }

    // Что получится, если применить изменение к тыкве
    public bool TryGetResult(ItemDefinition source, Ornament change, out ItemDefinition result)
    {
        result = null;
        if (source == null || change == null) return false;

        foreach (Recipe recipe in recipes)
        {
            if (recipe == null || recipe.source != source || recipe.result == null)
                continue;

            if (recipe.change != null && recipe.change.SameChangeAs(change))
            {
                result = recipe.result;
                return true;
            }
        }
        return false;
    }
}
