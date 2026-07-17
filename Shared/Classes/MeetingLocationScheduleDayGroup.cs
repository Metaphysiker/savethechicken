namespace Shared.Classes;

public class MeetingLocationScheduleDayGroup
{
    public DateTime Date { get; set; }
    public List<MeetingLocationScheduleEntry> Locations { get; set; } = new();
}
