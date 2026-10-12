public readonly struct MailMessageData
{
    public string Id { get; }
    public int Day { get; }
    public string Subject { get; }
    public string Body { get; }

    public MailMessageData(string id, int day, string subject, string body)
    {
        Id = id;
        Day = day;
        Subject = subject;
        Body = body;
    }
}

public readonly struct MailReceivedEvent
{
    public MailMessageData Message { get; }

    public MailReceivedEvent(MailMessageData message)
    {
        Message = message;
    }
}
