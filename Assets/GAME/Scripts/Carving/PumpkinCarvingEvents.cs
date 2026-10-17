// Станок резьбы просит открыть окно.
public readonly struct PumpkinCarvingOpenedEvent
{
    public PumpkinCarvingCatalog Catalog { get; }

    public PumpkinCarvingOpenedEvent(PumpkinCarvingCatalog catalog)
    {
        Catalog = catalog;
    }
}

// Тыкву изменили: сменили лицо и/или добавили/убрали свечу.
public readonly struct PumpkinCarvedEvent
{
    public PumpkinItemDefinition Source { get; }   // какой была
    public PumpkinItemDefinition Result { get; }   // какой стала

    public bool FaceChanged => Source.Face != Result.Face;
    public bool CandleAdded => !Source.HasCandle && Result.HasCandle;
    public bool CandleRemoved => Source.HasCandle && !Result.HasCandle;

    public PumpkinCarvedEvent(PumpkinItemDefinition source, PumpkinItemDefinition result)
    {
        Source = source;
        Result = result;
    }
}
