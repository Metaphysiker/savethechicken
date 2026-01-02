using Shared.Dtos.DtosImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Classes
{
    public class DriverRoutePlan
    {
        public DriverDto Driver { get; set; }
        public List<SaveChickenRequestDto> SaveChickenRequests { get; set; }

        public List<MapMarker> RouteMarkers { get; set; } = new List<MapMarker>();

        public List<MapArrow> RouteArrows { get; set; } = new List<MapArrow>();

        public DriverRoutePlan() { 
            Driver = new DriverDto();
            SaveChickenRequests = new List<SaveChickenRequestDto>();
        }

    }
}
