using System;
using System.Collections.Generic;
using UnityEngine;

public class MailboxService : MonoBehaviour
{
    private readonly List<MailMessageData> messages =
        new List<MailMessageData>();

    private readonly HashSet<string> readMessageIds =
        new HashSet<string>();

    public IReadOnlyList<MailMessageData> Messages => messages;

    public bool HasUnreadMessages
    {
        get
        {
            foreach (MailMessageData message in messages)
            {
                if (!readMessageIds.Contains(message.Id))
                    return true;
            }

            return false;
        }
    }

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
        if (string.IsNullOrWhiteSpace(message.Id))
        {
            Debug.LogWarning("MailboxService: Message ID cannot be empty.", this);
            return;
        }

        foreach (MailMessageData existing in messages)
        {
            if (existing.Id == message.Id)
                return;
        }

        // A new daily order email supersedes older daily order emails.
        if (IsDailyOrderMail(message))
        {
            foreach (MailMessageData existing in messages)
            {
                if (IsDailyOrderMail(existing) &&
                    existing.Day <= message.Day)
                {
                    readMessageIds.Add(existing.Id);
                }
            }
        }

        messages.Add(message);

        GameEvents.Publish(new MailReceivedEvent(message));
    }

    public bool TryGetNextUnread(out MailMessageData message)
    {
        // Prefer the latest unread daily order email.
        MailMessageData latestDailyMail = default;
        bool foundDailyMail = false;

        foreach (MailMessageData candidate in messages)
        {
            if (readMessageIds.Contains(candidate.Id))
                continue;

            if (!IsDailyOrderMail(candidate))
                continue;

            if (!foundDailyMail || candidate.Day > latestDailyMail.Day)
            {
                latestDailyMail = candidate;
                foundDailyMail = true;
            }
        }

        if (foundDailyMail)
        {
            message = latestDailyMail;
            return true;
        }

        // Other types of mail are handled normally.
        foreach (MailMessageData candidate in messages)
        {
            if (readMessageIds.Contains(candidate.Id))
                continue;

            message = candidate;
            return true;
        }

        message = default;
        return false;
    }

    public bool MarkRead(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return false;

        foreach (MailMessageData message in messages)
        {
            if (message.Id != messageId)
                continue;

            if (readMessageIds.Contains(messageId))
                return false;

            if (IsDailyOrderMail(message))
            {
                // Reading today's orders also expires older daily order emails.
                foreach (MailMessageData existing in messages)
                {
                    if (IsDailyOrderMail(existing) &&
                        existing.Day <= message.Day)
                    {
                        readMessageIds.Add(existing.Id);
                    }
                }
            }
            else
            {
                readMessageIds.Add(messageId);
            }

            GameEvents.Publish(new MailReadEvent(message));
            return true;
        }

        return false;
    }

    private bool IsDailyOrderMail(MailMessageData message)
    {
        return !string.IsNullOrEmpty(message.Id) &&
               message.Id.StartsWith(
                   "daily-orders-",
                   StringComparison.Ordinal
               );
    }
}