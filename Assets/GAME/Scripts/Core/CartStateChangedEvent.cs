public readonly struct CartStateChangedEvent
{
    public bool HasCart { get; }
    public bool IsHitched { get; }

    public CartStateChangedEvent(bool hasCart, bool isHitched)
    {
        HasCart = hasCart;
        IsHitched = hasCart && isHitched;
    }
}
