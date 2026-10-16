using UnityEngine;

/// <summary>
/// Completes after the player has travelled a configurable distance while the
/// cart is hitched. Distance travelled while the cart is parked is not counted.
/// </summary>
public class TravelWithCartCondition : TutorialCondition
{
    [Header("Movement Requirement")]
    [SerializeField, Min(0.1f)] private float requiredDistance = 3f;

    private TutorialStep activeStep;
    private Transform playerTransform;
    private CartService cartService;
    private Vector3 previousPosition;
    private float distanceTravelled;
    private bool initialized;

    public override bool IsMet(TutorialStep step)
    {
        if (step == null || !step.isActive)
            return false;

        if (activeStep != step)
        {
            activeStep = step;
            playerTransform = null;
            cartService = null;
            distanceTravelled = 0f;
            initialized = false;
        }

        if (playerTransform == null && step.Player != null)
            playerTransform = step.Player.transform;

        if (cartService == null)
        {
            GameServices services = GameServices.Instance;
            if (services != null)
                services.TryGet<CartService>(out cartService);
        }

        if (playerTransform == null || cartService == null)
            return false;

        Vector3 currentPosition = playerTransform.position;

        if (!initialized)
        {
            previousPosition = currentPosition;
            initialized = true;
            return false;
        }

        float frameDistance = Vector2.Distance(
            new Vector2(previousPosition.x, previousPosition.y),
            new Vector2(currentPosition.x, currentPosition.y));

        // Always advance the position baseline so walking before hitching
        // the cart cannot count toward the requirement.
        previousPosition = currentPosition;

        if (cartService.IsHitched)
            distanceTravelled += frameDistance;

        return distanceTravelled >= requiredDistance;
    }
}
