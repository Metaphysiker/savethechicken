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
            var resource = "";
            if (typeof(TDto).Name.EndsWith(typeof(SaveChickenRequestDto).Name))
            {
                resource = "SaveChickenRequest";
            }
            else if (typeof(TDto).Name.EndsWith(typeof(DriverDto).Name))
            {
                resource = "Driver";
            }
            else if (typeof(TDto).Name.EndsWith(typeof(FarmDto).Name))
            {
                resource = "Farm";
            }
            else if (typeof(TDto).Name.EndsWith(typeof(SaveChickenActionDto).Name))
            {
                resource = "SaveChickenAction";
            }
            return new GenericDtoService<TDto, TSearchDto>(_httpClient, resource);
        }
    }
}
