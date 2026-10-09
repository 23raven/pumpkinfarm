using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class PostManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform TodayDeliversContent;
    [SerializeField] private Transform SelectedDeliverContent;
    [SerializeField] private Transform CartSpawnPos;

    [SerializeField] private GameObject TodayDeliversPrefab;
    [SerializeField] private GameObject SelectedDeliverPrefab;
    [SerializeField] private GameObject CartPrefab;

    public List<Post> TodayDelivers = new();
    public Post SelectedPost;
    public bool Delivering;
    public bool WithCart;
    private GameObject CurrentCart;
    public Post CurrentDelivering;

    public void RenderTodayDelivers()
    {
        ClearTodayDelivers();

        for (int i = 0; i < TodayDelivers.Count; i++)
        {
            GameObject instance = Instantiate(TodayDeliversPrefab, TodayDeliversContent);

            instance.transform.GetChild(2).GetComponent<TMP_Text>().text = "";
            for (int j = 0; j < TodayDelivers[i].requirments.Count; j++)
            {
                instance.transform.GetChild(2).GetComponent<TMP_Text>().text += TodayDelivers[i].requirments[j].item.ornament.ToString() + " " + TodayDelivers[i].requirments[j].item.Itemname + ": " + TodayDelivers[i].requirments[j].quantity + "\n";
            }

            instance.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = TodayDelivers[i].icon;
            instance.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = TodayDelivers[i].reciever;

            instance.GetComponent<TodayDeliverPrefabScript>().postManager = this;
            instance.GetComponent<TodayDeliverPrefabScript>().post = TodayDelivers[i];

            instance.GetComponent<Button>().onClick.AddListener(instance.GetComponent<TodayDeliverPrefabScript>().Select);
        }
    }
    private void ClearTodayDelivers()
    {
        for (int i = TodayDeliversContent.childCount - 1; i >= 0; i--)
        {
            Destroy(TodayDeliversContent.GetChild(i).gameObject);
        }
    }

    private void ClearSelectedReqs()
    {
        for (int i = SelectedDeliverContent.childCount - 1; i >= 0; i--)
        {
            Destroy(SelectedDeliverContent.GetChild(i).gameObject);
        }
    }

    public void RenderSelectedDeliver()
    {
        ClearSelectedReqs();

        if (SelectedPost != null)
        {
            SelectedDeliverContent.parent.parent.parent.GetChild(2).GetComponent<TMP_Text>().text = "Receiver: " + SelectedPost.reciever;
            SelectedDeliverContent.parent.parent.parent.GetChild(4).GetChild(0).gameObject.GetComponent<Image>().sprite = SelectedPost.icon;
            SelectedDeliverContent.parent.parent.parent.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "Cost: " + SelectedPost.cost.ToString();

            for (int j = 0; j < SelectedPost.requirments.Count; j++)
            {
                GameObject instance = Instantiate(SelectedDeliverPrefab, SelectedDeliverContent);
                instance.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Image>().sprite = SelectedPost.requirments[j].item.icon;
                instance.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = SelectedPost.requirments[j].item.Itemname;
                instance.transform.GetChild(2).gameObject.GetComponent<TMP_Text>().text = SelectedPost.requirments[j].item.ornament.ToString();
            }
        }
        else
        {
            SelectedDeliverContent.parent.parent.parent.GetChild(2).GetComponent<TMP_Text>().text = "";
            SelectedDeliverContent.parent.parent.parent.GetChild(4).GetChild(0).gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Emptiness");
            SelectedDeliverContent.parent.parent.parent.GetChild(3).gameObject.GetComponent<TMP_Text>().text = "";
        }
    }

    private void OnEnable()
    {
        RenderTodayDelivers();
    }
    public void SendPost()
    {
        if (SelectedPost == null) { return; }

        bool hasAllItems = true;
        for (int i = 0; i < SelectedPost.requirments.Count; i++)
        {
            if (!inventoryManager.HasItem(SelectedPost.requirments[i].item, SelectedPost.requirments[i].quantity))
            {
                hasAllItems = false;
                break;
            }
        }

        if (hasAllItems)
        {
            for (int i = 0; i < CurrentDelivering.requirments.Count; i++)
            {
                inventoryManager.RemoveItem(CurrentDelivering.requirments[i].item, CurrentDelivering.requirments[i].quantity);
            }

            if (CurrentCart == null)
            {
                GameObject instance = Instantiate(CartPrefab, CartSpawnPos.position, Quaternion.identity);
                CurrentCart = instance;
            }

            Delivering = true;
            CurrentDelivering = SelectedPost;
        }
    }

    public void GetPostAward()
    {
        Destroy(CurrentCart);
        CurrentCart = null;
        WithCart = false;

        gameManager.AddMoney(CurrentDelivering.cost);

        TodayDelivers.Remove(CurrentDelivering);
        SelectedPost = null;
        CurrentDelivering = null;
        Delivering = false;

        RenderSelectedDeliver();
        RenderTodayDelivers();
    }
}
[System.Serializable]
public class Post
{
    public string reciever;
    public Sprite icon;
    public int cost;
    public List<Requitment> requirments = new();
    public Transform DeliverPos;
}

[System.Serializable]
public class Requitment
{
    public InvItem item;
    public int quantity;
}