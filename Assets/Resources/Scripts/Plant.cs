using UnityEngine;
using System.Collections.Generic;
using System;

public class Plant : MonoBehaviour
{
    [SerializeField] private List<Sprite> stages = new();
    [SerializeField] private Sprite NoneStage;
    public int stage;
    public bool planted;

    public void Grow()
    {
        stage++;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = stages[stage];
    }

    public void PickUp()
    {
        planted = false;
        stage = 0;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = NoneStage;
    }

    public void PutDown()
    {
        stage = 0;
        planted = true;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = stages[stage];
    }
}
