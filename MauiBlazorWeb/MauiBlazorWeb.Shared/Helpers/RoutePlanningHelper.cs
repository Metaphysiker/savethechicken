using MauiBlazorWeb.Shared.Classes;
using Shared.Dtos.DtosImpl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MauiBlazorWeb.Shared.Helpers
{
    /// <summary>
    /// Helper class for automatic route planning logic
    /// Distributes rescue requests among available drivers based on capacity and dates
    /// </summary>
    public static class RoutePlanningHelper
    {
        /// <summary>
        /// Automatically generates drive plans by distributing requests among drivers
        /// ALGORITHM:
        /// - Groups requests by postal code
        /// - Assigns drivers for each date based on availability
        /// - Fills each driver's capacity before moving to next driver
        /// - Creates separate plans for each farm
        /// </summary>
        /// <param name="requests">All rescue requests to be assigned</param>
        /// <param name="drivers">Available drivers with their capacities and dates</param>
        /// <param name="farms">Farms to use as starting points</param>
        /// <param name="dates">Unique rescue dates from the action</param>
        /// <returns>List of drive plans with requests distributed among drivers</returns>
        public static List<DrivePlan> PlanRoutes(
            List<SaveChickenRequestDto> requests,
            List<SaveChickenDriveRequestDto> drivers,
            List<FarmDto> farms,
            HashSet<DateOnly> dates)
        {
            var drivePlans = new List<DrivePlan>();
            var copyOfRequests = new List<SaveChickenRequestDto>(requests);

            foreach (var date in dates)
            {
                foreach (var farm in farms)
                {
                    // Sort requests by postal code for efficient routing
                    var sortedRequests = copyOfRequests
                        .Where(r => r.Person?.Address != null && !string.IsNullOrWhiteSpace(r.Person.Address.PostalCode))
                        .OrderBy(r => r.Person!.Address!.PostalCode)
                        .ToList();

                    // Get drivers available for this specific date
                    var driversForDate = drivers.Where(d => d.AvailableDates.Contains(date)).ToList();
                    var driversCount = driversForDate.Count;

                    // Skip if no drivers available or no requests to assign
                    if (driversCount == 0 || sortedRequests.Count == 0)
                        continue;

                    var currentDriverIndex = 0;
                    DrivePlan? currentDrivePlan = null;

                    // Distribute requests among drivers
                    for (int i = 0; i < sortedRequests.Count; i++)
                    {
                        var request = sortedRequests[i];

                        // Skip if already assigned to another drive plan
                        if (drivePlans.Any(drp => drp.SaveChickenRequests.Any(r => r.Id == request.Id)))
                            continue;

                        var driver = driversForDate[currentDriverIndex];

                        // Calculate total animals if we add this request to current route
                        var sumOfAnimalsInCurrentRoute = currentDrivePlan != null
                            ? currentDrivePlan.SaveChickenRequests.Sum(r => r.NumberOfChickensToBeSaved + r.NumberOfRoostersToBeSaved)
                            : 0;

                        var totalAnimals = request.NumberOfChickensToBeSaved + request.NumberOfRoostersToBeSaved + sumOfAnimalsInCurrentRoute;

                        // CAPACITY CHECK: Would this request exceed driver's capacity?
                        if (totalAnimals > driver.CapacityForChickens)
                        {
                            // Move to next driver (round-robin)
                            currentDriverIndex = (currentDriverIndex + 1) % driversCount;
                            driver = driversForDate[currentDriverIndex];
                            currentDrivePlan = null; // Force creation of new plan
                        }

                        // Create new drive plan if needed
                        if (currentDrivePlan == null)
                        {
                            int numberOfDrive = drivePlans.Count;
                            currentDrivePlan = new DrivePlan
                            {
                                Driver = driver,
                                SaveChickenRequests = new List<SaveChickenRequestDto>(),
                                RouteDate = date,
                                Farm = farm,
                                NumberOfDrive = numberOfDrive
                            };
                            drivePlans.Add(currentDrivePlan);
                        }

                        // Add request to current plan
                        currentDrivePlan.SaveChickenRequests.Add(request);
                    }
                }
            }

            return drivePlans;
        }

        /// <summary>
        /// Extracts all unique rescue dates from a SaveChickenAction
        /// </summary>
        /// <param name="saveChickenActionDto">The action containing dates</param>
        /// <returns>HashSet of unique dates, or empty set if no dates exist</returns>
        public static HashSet<DateOnly> GetAllUniqueRescueDates(SaveChickenActionDto saveChickenActionDto)
        {
            if (saveChickenActionDto?.Dates == null || !saveChickenActionDto.Dates.Any())
                return new HashSet<DateOnly>();

            return saveChickenActionDto.Dates.ToHashSet();
        }
    }
}
