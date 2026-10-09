using UnityEngine;
using System.Collections.Generic;

public class TriggerController : MonoBehaviour
{
    //тут прописывать все взаимодействия игрока с помощью тригеров и у меня стоит пробел как interact тут

    [SerializeField] private GameManager gameManager;
    [SerializeField] private InventoryManager invManager;
    [SerializeField] private DialogRenderer dialogRenderer;
    [SerializeField] private PostManager postManager;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Sell sell;
    [SerializeField] private GameObject ShopUiObj;
    [SerializeField] private GameObject CarvingUiObj;
    [SerializeField] private GameObject EventBoardObjUi;
    [SerializeField] private GameObject PostUiObj;
    [SerializeField] private Sprite PlowedSoilSprite;
    [SerializeField] private Transform PlayerTransform;

    [SerializeField] private List<Plant> CurrPlants = new();
    private bool NearBed;
    private bool NearShop;
    private bool NearCarvingTable;
    private bool NearEventBoard;
    private bool NearPostArea;
    private GameObject NearDeliverArea;
    private Dialog dialog;
    private CartFollow cart;
    [SerializeField] private List<GameObject> NearItem = new();
    [SerializeField] private string PumpkinSeedsName;
    [SerializeField] private string WateringCanName;
    [SerializeField] private string HoeName;

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
        else if (other.CompareTag("EventArea"))
        {
            NearEventBoard = true;
        }
        else if (other.CompareTag("PostArea"))
        {
            NearPostArea = true;
        }
        else if (other.CompareTag("Cart"))
        {
            cart = other.gameObject.GetComponent<CartFollow>();
        }
        else if (other.CompareTag("DeliverArea"))
        {
            NearDeliverArea = other.gameObject;
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
        else if (other.CompareTag("EventArea"))
        {
            NearEventBoard = false;
        }
        else if (other.CompareTag("PostArea"))
        {
            NearPostArea = false;
        }
        else if (other.CompareTag("Cart"))
        {
            cart = null;
        }
        else if (other.CompareTag("DeliverArea"))
        {
            NearDeliverArea = null;
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

            if (NearCarvingTable)
            {
                movement.enabled = !movement.enabled;
                CarvingUiObj.SetActive(!CarvingUiObj.activeSelf);
            }

            if (NearEventBoard)
            {
                movement.enabled = !movement.enabled;
                EventBoardObjUi.SetActive(!EventBoardObjUi.activeSelf);
            }

            if (NearPostArea)
            {
                movement.enabled = !movement.enabled;
                PostUiObj.SetActive(!PostUiObj.activeSelf);
            }

            if (NearDeliverArea != null)
            {
                if (postManager.Delivering && postManager.WithCart)
                {
                    if (postManager.CurrentDelivering.DeliverPos.gameObject.name == NearDeliverArea.name)
                    {
                        postManager.GetPostAward();
                    }
                }
            }

            if (cart != null)
            {
                cart.enabled = !cart.enabled;
                cart.SetTarget(PlayerTransform);

                if (postManager.Delivering)
                {
                    postManager.WithCart = !postManager.WithCart;
                }
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
                else if (!currentPlant.planted && currentPlant.plowed)
                {
                    if (invManager.hotbar[invManager.CSlot].item != null &&
                        invManager.hotbar[invManager.CSlot].item.Itemname == PumpkinSeedsName)
                    {
                        currentPlant.PutDown();
                    }
                }

                if (invManager.hotbar[invManager.CSlot].item != null &&
                    invManager.hotbar[invManager.CSlot].item.Itemname == WateringCanName && currentPlant.plowed)
                {
                    currentPlant.watered = true;
                    currentPlant.transform.parent.gameObject.GetComponent<SpriteRenderer>().color = new Color32(77, 77, 77, 255);
                }

                if (invManager.hotbar[invManager.CSlot].item != null &&
                    invManager.hotbar[invManager.CSlot].item.Itemname == HoeName)
                {
                    currentPlant.plowed = true;
                    currentPlant.transform.parent.GetComponent<SpriteRenderer>().sprite = PlowedSoilSprite;
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
