using UnityEngine;

public class PlayerItemDropController : MonoBehaviour
{
    [Header("Drop Settings")]
    [SerializeField, Min(0.1f)]
    private float dropDistance = 0.9f;

    private IDirectionalInteractor directionalInteractor;

    private void Awake()
    {
        directionalInteractor =
            GetComponent(typeof(IDirectionalInteractor))
            as IDirectionalInteractor;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            DropSelectedItem();
    }

    private void DropSelectedItem()
    {
        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<InventoryService>(
                out InventoryService inventory))
        {
            Debug.LogWarning("InventoryService is unavailable.", this);
            return;
        }

        ItemDefinition selectedItem = inventory.SelectedItem;

        if (selectedItem == null)
        {
            Debug.Log("Selected slot is empty.");
            return;
        }

        if (selectedItem.WorldPrefab == null)
        {
            Debug.LogWarning(
                $"No world prefab assigned for {selectedItem.DisplayName}.",
                this
            );
            return;
        }

        Vector2 direction = directionalInteractor != null
            ? directionalInteractor.FacingDirection
            : Vector2.down;

        if (direction == Vector2.zero)
            direction = Vector2.down;

        Vector3 spawnPosition = transform.position +
            (Vector3)(direction.normalized * dropDistance);

        PickupItem prefab = selectedItem.WorldPrefab;

        if (!inventory.TryRemoveSelectedSlot(
            out ItemDefinition item,
            out int quantity))
        {
            Debug.Log("Selected slot is empty.");
            return;
        }

        PickupItem droppedItem = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        droppedItem.Initialize(item, quantity);

        Debug.Log($"Dropped {item.DisplayName} x{quantity}.");
    }
}