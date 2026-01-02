namespace Availability.Domain.Entities;

public class TimeSlot
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public TimeSlot(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    public bool Overlaps(TimeSlot other)
    {
        return Start < other.End && End > other.Start;
    }
}
