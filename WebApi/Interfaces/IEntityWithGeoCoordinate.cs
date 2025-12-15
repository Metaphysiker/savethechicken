using Shared.Dtos.DtosImpl;

namespace WebApi.Interfaces
{
    public interface IEntityWithGeoCoordinate
    {
        public GeoCoordinate? GeoCoordinate { get; set; }
    }
}
