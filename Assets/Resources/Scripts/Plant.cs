using UnityEngine;
using System.Collections.Generic;
using System;

public class Plant : MonoBehaviour
{
    [SerializeField] private List<Sprite> stages = new();
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private Sprite NoneStage;
    public int stage;
    public bool planted;
    public bool watered;
    public bool dead;

    public void Grow()
    {
        if (stage != 6)
        {
            stage++;
            this.gameObject.GetComponent<SpriteRenderer>().sprite = stages[stage];
        }
    }

    public void PickUp()
    {
        planted = false;
        dead = false;
        stage = 0;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = NoneStage;
    }

    public void PutDown()
    {
        stage = 0;
        planted = true;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = stages[stage];
    }

    public void Death()
    {
        this.gameObject.GetComponent<SpriteRenderer>().sprite = deathSprite;
        stage = 6;
        dead = true;
    }
}
