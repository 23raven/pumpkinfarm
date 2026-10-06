using UnityEngine;
using System.Collections.Generic;
using System;

public class Plant : MonoBehaviour
{
    [SerializeField] private List<Sprite> stages = new();
    [SerializeField] private Sprite deathSprite;
    [SerializeField] private Sprite NoneStage;
    [SerializeField] private BigStage bigPumpkin = new();
    [SerializeField] private BigStage basic = new();
    public int stage;
    public bool planted;
    public bool watered;
    public bool dead;

    //это Pumpkin который будет появляться на последнем Stage и является pickUp предметом
    [SerializeField] private GameObject pumpkinItem;

    public void Grow()
    {
        if (stage != 6)
        {
            stage++;
            this.gameObject.GetComponent<SpriteRenderer>().sprite = stages[stage];
        }

        if (stage == 6 && !dead)
        {
            pumpkinItem.SetActive(true);
        }
    }

    public void ChangeForBig()
    {
        this.transform.localScale = bigPumpkin.scale;
        this.transform.localPosition = bigPumpkin.pos;
        this.GetComponent<SpriteRenderer>().sprite = bigPumpkin.sprite;
        this.GetComponent<BoxCollider2D>().offset = bigPumpkin.offset;
        this.GetComponent<BoxCollider2D>().size = bigPumpkin.size;
        this.transform.parent.GetChild(0).GetComponent<BoxCollider2D>().size = bigPumpkin.size2;
        this.transform.parent.GetChild(0).GetComponent<BoxCollider2D>().offset = bigPumpkin.offset2;
        this.transform.parent.GetChild(0).GetComponent<PickUpItem>().quantity = 8;
    }

    public void ChangeForBasic()
    {
        this.transform.localScale = basic.scale;
        this.transform.localPosition = basic.pos;
        this.GetComponent<SpriteRenderer>().sprite = basic.sprite;
        this.GetComponent<BoxCollider2D>().offset = basic.offset;
        this.GetComponent<BoxCollider2D>().size = basic.size;
        this.transform.parent.GetChild(0).GetComponent<BoxCollider2D>().size = basic.size2;
        this.transform.parent.GetChild(0).GetComponent<BoxCollider2D>().offset = basic.offset2;
        this.transform.parent.GetChild(0).GetComponent<PickUpItem>().quantity = 1;
    }

    public void PickUp()
    {
        planted = false;
        dead = false;
        stage = 0;
        this.gameObject.GetComponent<SpriteRenderer>().sprite = NoneStage;
        ChangeForBasic();

        pumpkinItem.SetActive(false);
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

[System.Serializable]
public class BigStage
{
    public Sprite sprite;
    public Vector2 pos;
    public Vector2 scale;
    public Vector2 size; //размер коллайдера
    public Vector2 offset; //оффсет колайдера
    public Vector2 size2; //размер коллайдера предмета
    public Vector2 offset2; //оффсет коллайдера предмета
}
