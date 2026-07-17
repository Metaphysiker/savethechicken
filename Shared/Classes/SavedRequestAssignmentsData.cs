namespace Shared.Classes;

public class SavedRequestAssignmentsData
{
        public int Version { get; set; }
        public List<MeetingLocation> MeetingLocations { get; set; } = new();
        public List<RequestAssignmentJson> Assignments { get; set; } = new();
}
