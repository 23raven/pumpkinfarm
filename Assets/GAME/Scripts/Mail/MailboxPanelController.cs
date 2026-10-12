using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailboxPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text subjectText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button readButton;

    private MailMessageData currentMessage;
    private bool hasCurrentMessage;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.Subscribe<MailboxOpenRequestedEvent>(
            OnMailboxOpenRequested
        );

        if (readButton != null)
            readButton.onClick.AddListener(ReadCurrentMessage);
    }

    private void OnDisable()
    {
        GameEvents.Unsubscribe<MailboxOpenRequestedEvent>(
            OnMailboxOpenRequested
        );

        if (readButton != null)
            readButton.onClick.RemoveListener(ReadCurrentMessage);

        GameplayInputGate.SetBlocked(false);
    }

    private void OnMailboxOpenRequested(MailboxOpenRequestedEvent message)
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("Mailbox panel is not assigned.", this);
            return;
        }

        if (panelRoot.activeSelf)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<MailboxService>(out MailboxService mailbox) ||
            !mailbox.TryGetNextUnread(out currentMessage))
        {
            Debug.Log("No unread mail to display.");
            return;
        }

        hasCurrentMessage = true;

        if (subjectText != null)
            subjectText.text = currentMessage.Subject;

        if (bodyText != null)
            bodyText.text = currentMessage.Body;

        panelRoot.SetActive(true);
        GameplayInputGate.SetBlocked(true);
    }

    private void ReadCurrentMessage()
    {
        if (!hasCurrentMessage)
            return;

        GameServices services = GameServices.Instance;

        if (services == null ||
            !services.TryGet<MailboxService>(out MailboxService mailbox))
        {
            Debug.LogWarning("MailboxService is unavailable.", this);
            return;
        }

        bool markedRead = mailbox.MarkRead(currentMessage.Id);

        if (!markedRead)
        {
            Debug.LogWarning(
                "Could not mark the current message as read.",
                this
            );
            return;
        }

        ClosePanel();
    }

    private void ClosePanel()
    {
        hasCurrentMessage = false;
        GameplayInputGate.SetBlocked(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }
}