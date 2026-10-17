using System.Collections;
using TMPro;
using UnityEngine;

public readonly struct InsufficientItemsWarningEvent
{
    public string Message { get; }

    public InsufficientItemsWarningEvent(string message)
    {
        Message = message;
    }
}

/// <summary>
/// Listens for failed order deliveries and briefly shows a TextMeshPro warning.
/// Attach this component to an always-active object, not to the warning text itself.
/// </summary>
public class InsufficientItemsWarningDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text warningText;
    [SerializeField, Min(0.1f)] private float displayDuration = 5f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (warningText != null)
            warningText.gameObject.SetActive(false);
        else
            Debug.LogWarning("InsufficientItemsWarningDisplay: Warning Text is not assigned.", this);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<InsufficientItemsWarningEvent>(OnWarningRequested);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<InsufficientItemsWarningEvent>(OnWarningRequested);

        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }
    }

    private void OnWarningRequested(InsufficientItemsWarningEvent message)
    {
        if (warningText == null)
            return;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        warningText.text = string.IsNullOrWhiteSpace(message.Message)
            ? "Not enough items for this order!"
            : message.Message;

        warningText.gameObject.SetActive(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        hideRoutine = null;
    }
}
