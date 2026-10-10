using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 1.5f;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
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
                IInteractable interactable =
                    component as IInteractable;

                if (interactable == null)
                    continue;

                float distance = Vector2.Distance(
                    transform.position,
                    hit.ClosestPoint(transform.position)
                );

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = interactable;
                }
            }
        }

        if (nearest != null)
            nearest.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            interactionRadius
        );
    }
}