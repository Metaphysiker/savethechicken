using System;
using System.Net.Http;
using MauiBlazorWeb.Web.Services.ServicesImpl;
using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Shared.Factories.FactoriesImpl
{
    public class GenericDtoServiceFactory
    {
        private readonly HttpClient _httpClient;

        public GenericDtoServiceFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public GenericDtoService<TDto, TSearchDto> Create<TDto, TSearchDto>()
            where TDto : class
            where TSearchDto : class
        {
            var resource = typeof(TDto) switch
            {
                var t when t == typeof(SaveChickenRequestDto) => "SaveChickenRequest",
                var t when t == typeof(SaveChickenDriveRequestDto) => "SaveChickenDriveRequest",
                var t when t == typeof(SaveChickenActionFarmDto) => "SaveChickenActionFarm",
                var t when t == typeof(FarmDto) => "Farm",
                var t when t == typeof(SaveChickenActionDto) => "SaveChickenAction",
                var t when t == typeof(PersonDto) => "Person",
                var t when t == typeof(StoredFileDto) => "File",
                _ => throw new InvalidOperationException($"No resource mapping configured for type '{typeof(TDto).Name}'.")
            };

            return new GenericDtoService<TDto, TSearchDto>(_httpClient, resource);
        }
    }
}
