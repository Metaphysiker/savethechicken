namespace Shared.Classes;

public class MeetingLocationScheduleEntry
{
    public int SequenceInDay { get; set; } // 1-based position among that day's locations, ordered by time
    public string Name { get; set; } = "";
    public DateTime DateTime { get; set; }
    public List<AssignedRequestEntry> AssignedRequests { get; set; } = new();

}
