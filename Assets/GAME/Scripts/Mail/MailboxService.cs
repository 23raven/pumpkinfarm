using System.Collections.Generic;
using UnityEngine;

public class MailboxService : MonoBehaviour
{
    private readonly List<MailMessageData> messages = new List<MailMessageData>();

    public IReadOnlyList<MailMessageData> Messages => messages;

    private void OnEnable()
    {
        GameServices services = GameServices.Instance;
        if (services == null)
        {
            Debug.LogError("MailboxService: GameServices is missing.", this);
            return;
        }

        services.Register(this);
    }

    private void OnDisable()
    {
        GameServices services = GameServices.Instance;
        if (services != null)
            services.Unregister(this);
    }

    public void Receive(MailMessageData message)
    {
        messages.Add(message);
        GameEvents.Publish(new MailReceivedEvent(message));
    }
}
