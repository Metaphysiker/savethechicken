using Shared.Dtos.DtosImpl;

namespace Shared.Classes;

public class MeetingLocation
{
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "";
        public DateTime DateTime { get; set; } = DateTime.Today;
        public List<int> DriverIds { get; set; } = new();
        public GeoCoordinate? GeoCoordinate { get; set; }
}
