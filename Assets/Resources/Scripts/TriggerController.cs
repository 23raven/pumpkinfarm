using UnityEngine;
using System.Collections.Generic;

public class TriggerController : MonoBehaviour
{
    //тут прописывать все взаимодействия игрока с помощью тригеров и у меня стоит пробел как interact тут

    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager invManager;
    [SerializeField] private DialogRenderer dialogRenderer;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Sell sell;
    [SerializeField] private GameObject ShopUiObj;

    [SerializeField] private List<Plant> CurrPlants = new();
    private bool NearBed;
    private bool NearShop;
    private bool NearCarvingTable;
    private Dialog dialog;
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
        else if (other.CompareTag("SellArea"))
        {
            NearShop = true;
        }
        else if (other.CompareTag("CarvingArea"))
        {
            NearCarvingTable = true;
        }
        else if (other.CompareTag("npc"))
        {
            dialog = other.gameObject.GetComponent<Npc>().dialog;
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
        else if (other.CompareTag("SellArea"))
        {
            NearShop = false;
        }
        else if (other.CompareTag("CarvingArea"))
        {
            NearCarvingTable = false;
        }
        else if (other.CompareTag("npc"))
        {
            dialog = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (NearBed)
            {
                gameManager.NextDay();
                return;
            }

            if (NearShop)
            {
                ShopUiObj.SetActive(!ShopUiObj.activeSelf);
                movement.enabled = !movement.enabled;
                sell.DrawSell();
                sell.DrawBuy();
                return;
            }

            if (dialog != null)
            {
                dialogRenderer.CurrentPhrase = 0;
                dialogRenderer.Current_dialog = dialog;
                dialogRenderer.gameObject.SetActive(true);
            }

            if (CurrPlants.Count > 0)
            {
                Plant currentPlant = CurrPlants[0];

                if (currentPlant.stage == 6)
                {
                    PickUpItem itemScript = currentPlant.transform.parent.GetChild(0).GetComponent<PickUpItem>();

                    if (itemScript != null)
                    {
                        invManager.AddItem(itemScript.item, itemScript.quantity);
                    }

                    currentPlant.PickUp();

                }
                else if (!currentPlant.planted)
                {
                    if (invManager.hotbar[invManager.CSlot].item != null &&
                        invManager.hotbar[invManager.CSlot].item.Itemname == PumpkinSeedsName)
                    {
                        currentPlant.PutDown();
                    }
                }

                if (invManager.hotbar[invManager.CSlot].item != null &&
                    invManager.hotbar[invManager.CSlot].item.Itemname == WateringCanName)
                {
                    currentPlant.watered = true;
                    currentPlant.transform.parent.gameObject.GetComponent<SpriteRenderer>().color = new Color32(77, 77, 77, 255);
                }
            }
            else if (NearItem.Count > 0)
            {
                invManager.AddItem(NearItem[0].GetComponent<PickUpItem>().item, NearItem[0].GetComponent<PickUpItem>().quantity);
                Destroy(NearItem[0]);
            }
        }
    }
}
