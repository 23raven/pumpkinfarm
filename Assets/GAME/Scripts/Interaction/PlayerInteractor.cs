using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform interactionPoint;
    [SerializeField, Min(0.1f)] private float interactionRadius = 0.8f;

    public Vector3 InteractionPosition =>
        interactionPoint != null
            ? interactionPoint.position
            : transform.position;

    private void Update()
    {
        if (GameplayInputGate.IsBlocked)
            return;

        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            InteractionPosition,
            interactionRadius
        );

        IInteractable nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            MonoBehaviour[] components =
                hit.GetComponentsInParent<MonoBehaviour>();

            foreach (MonoBehaviour component in components)
            {
                if (!(component is IInteractable interactable))
                    continue;

                float distance = Vector2.Distance(
                    InteractionPosition,
                    hit.ClosestPoint(InteractionPosition)
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = interactable;
                }
            }
        }

        if (nearest != null)
            nearest.Interact(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 position = interactionPoint != null
            ? interactionPoint.position
            : transform.position;

        Gizmos.DrawWireSphere(position, interactionRadius);
    }
}