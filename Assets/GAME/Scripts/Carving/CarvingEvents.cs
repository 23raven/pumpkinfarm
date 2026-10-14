// Станок резьбы просит открыть окно резьбы.
public readonly struct CarvingOpenedEvent
{
    public CarvingStation Station { get; }

    public CarvingOpenedEvent(CarvingStation station)
    {
        Station = station;
    }
}
