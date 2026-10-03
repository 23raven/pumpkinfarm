using UnityEngine;
using System.Collections.Generic;

public class TriggerController : MonoBehaviour
{
    //тут прописывать все взаимодействия игрока с помощью тригеров и у меня стоит пробел как interact тут

    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager invManager;

    [SerializeField] private List<Plant> CurrPlants = new();
    private bool NearBed;
    [SerializeField] private List<GameObject> NearItem = new();
    [SerializeField] private string PumpkinSeedsName;
    [SerializeField] private string WateringCanName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("FieldCell"))
        {
            CurrPlants.Add(other.GetComponent<Plant>());
        }
        else if (other.CompareTag("Bed"))
        {
            NearBed = true;
        }
        else if (other.CompareTag("Item"))
        {
            NearItem.Add(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("FieldCell"))
        {
            CurrPlants.Remove(other.GetComponent<Plant>());
        }
        else if (other.CompareTag("Bed"))
        {
            NearBed = false;
        }
        else if (other.CompareTag("Item"))
        {
            NearItem.Remove(other.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (CurrPlants.Count > 0)
            {
                if (CurrPlants[0].stage != 6)
                {
                    if (!CurrPlants[0].planted && invManager.hotbar[invManager.CSlot].item != null && invManager.hotbar[invManager.CSlot].item.Itemname == PumpkinSeedsName)
                    {
                        CurrPlants[0].PutDown();
                    }
                }
                else if (CurrPlants[0].stage == 6)
                {
                    CurrPlants[0].PickUp();
                }

                if (invManager.hotbar[invManager.CSlot].item != null && invManager.hotbar[invManager.CSlot].item.Itemname == WateringCanName)
                {
                    CurrPlants[0].watered = true;
                    CurrPlants[0].transform.parent.gameObject.GetComponent<SpriteRenderer>().color = new Color(0.24f, 0.15f, 0.08f);
                }
            }

            if (NearBed)
            {
                gameManager.NextDay();
            }

            if (NearItem.Count > 0)
            {
                invManager.AddItem(NearItem[0].GetComponent<PickUpItem>().item, NearItem[0].GetComponent<PickUpItem>().quantity);
                Destroy(NearItem[0]);
            }
        }
    }
}
