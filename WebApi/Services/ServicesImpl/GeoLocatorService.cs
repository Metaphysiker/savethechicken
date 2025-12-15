using Shared.Dtos.DtosImpl;
using System.Text.Json;
using WebApi.Factories.FactoriesImpl;
using WebApi.Interfaces;
using WebApi.Models.ModelsImpl;

namespace WebApi.Services.ServicesImpl
{
    public class GeoLocatorService

    {
        public async Task<GeoCoordinate?> GetCoordinatesAsync(
        string street, string zip, string city)
        {
            using var client = new HttpClient();

            var query = $"{street}, {zip} {city}";
            var url =
                $"https://api3.geo.admin.ch/rest/services/api/SearchServer" +
                $"?searchText={Uri.EscapeDataString(query)}" +
                $"&type=locations";

            try
            {
                var response = await client.GetFromJsonAsync<JsonElement>(url);
                if (!response.TryGetProperty("results", out var results) ||
                    results.GetArrayLength() == 0)
                    return null;

                var attrs = results[0].GetProperty("attrs");
                return new GeoCoordinate
                {
                    Latitude = attrs.GetProperty("lat").GetDouble(),
                    Longitude = attrs.GetProperty("lon").GetDouble()
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
