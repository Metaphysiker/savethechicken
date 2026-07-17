using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Classes
{
    public class MapMarker
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Info { get; set; } = string.Empty;
        public string Color { get; set; } = "blue"; // Default blue for regular markers
        public string? Id { get; set; }        // set to make marker interactive
        public string? PopupHtml { get; set; }  // custom popup content; falls back to Info if null
    }
}
