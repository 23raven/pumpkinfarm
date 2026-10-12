using UnityEngine;

public class MailboxStation : MonoBehaviour, IInteractable
{
    [Header("Unread Mail Indicator")]
    [SerializeField] private GameObject unreadIndicator;

    private void OnEnable()
    {
        GameEvents.Subscribe<MailReceivedEvent>(OnMailChanged);
        GameEvents.Subscribe<MailReadEvent>(OnMailChanged);
        RefreshIndicator();
    }

    private void Start()
    {
        // Services may finish registering after this component's OnEnable.
        RefreshIndicator();
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<MailReceivedEvent>(OnMailChanged);
        GameEvents.Unsubscribe<MailReadEvent>(OnMailChanged);
    }

    public void Interact(GameObject interactor)
    {
        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<MailboxService>(out MailboxService mailbox))
        {
            Debug.LogWarning("MailboxStation: MailboxService is unavailable.", this);
            return;
        }

        if (!mailbox.HasUnreadMessages)
        {
            Debug.Log("No unread mail.");
            RefreshIndicator();
            return;
        }

        GameEvents.Publish(new MailboxOpenRequestedEvent());
    }

    private void OnMailChanged(MailReceivedEvent message)
    {
        RefreshIndicator();
    }

    private void OnMailChanged(MailReadEvent message)
    {
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        if (unreadIndicator == null)
            return;

        GameServices services = GameServices.Instance;
        bool hasUnread = services != null &&
                         services.TryGet<MailboxService>(out MailboxService mailbox) &&
                         mailbox.HasUnreadMessages;

        unreadIndicator.SetActive(hasUnread);
    }
}
