using Shared.Dtos.DtosImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Classes
{
    public class DrivePlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int NumberOfDrive { get; set; } = 1;
        public FarmDto Farm { get; set; }
        public DriverDto Driver { get; set; }
        public DateOnly RouteDate { get; set; }
        public List<SaveChickenRequestDto> SaveChickenRequests { get; set; }

        public List<MapMarker> RouteMarkers { get; set; } = new List<MapMarker>();

        public List<MapArrow> RouteArrows { get; set; } = new List<MapArrow>();

        public DrivePlan() {
            Farm = new FarmDto();
            Driver = new DriverDto();
            SaveChickenRequests = new List<SaveChickenRequestDto>();
            RouteDate = DateOnly.FromDateTime(DateTime.Now);
        }

    }
}
