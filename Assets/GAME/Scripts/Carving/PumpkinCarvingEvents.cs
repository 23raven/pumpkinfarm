// Станок резьбы просит открыть окно.
public readonly struct PumpkinCarvingOpenedEvent
{
    public PumpkinCarvingCatalog Catalog { get; }

    public PumpkinCarvingOpenedEvent(PumpkinCarvingCatalog catalog)
    {
        Catalog = catalog;
    }
}
