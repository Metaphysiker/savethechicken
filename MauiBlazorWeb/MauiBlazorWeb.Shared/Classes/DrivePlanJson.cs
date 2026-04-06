using System;
using System.Collections.Generic;

namespace MauiBlazorWeb.Shared.Classes
{
    /// <summary>
    /// Simplified DrivePlan structure for JSON serialization
    /// Stores only IDs to avoid circular references and complex object graphs
    /// </summary>
    public class DrivePlanJson
    {
        public int Version { get; set; } = 1;
        public Guid Id { get; set; }
        public int NumberOfDrive { get; set; }
        public int FarmId { get; set; }
        public int DriverId { get; set; }
        public DateOnly RouteDate { get; set; }
        public List<int> SaveChickenRequestIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Wrapper for saved route plans with metadata
    /// Tracks all request IDs to detect truly new requests
    /// </summary>
    public class SavedRoutePlansData
    {
        public int Version { get; set; } = 1;
        public List<DrivePlanJson> Plans { get; set; } = new List<DrivePlanJson>();
        public List<int> AllRequestIdsAtSaveTime { get; set; } = new List<int>();
    }

    /// <summary>
    /// Result of loading saved plans with repair information
    /// </summary>
    public class LoadPlanResult
    {
        public bool HasData { get; set; }
        public int PlansLoaded { get; set; }
        public int PlansSkipped { get; set; }
        public int RequestsMoved { get; set; }
        public int NewRequestsDetected { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
