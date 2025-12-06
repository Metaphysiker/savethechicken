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
            Console.WriteLine($"Creating GenericDtoService for DTO type: {typeof(TDto).Name}");
            if (typeof(TDto).Name.EndsWith(typeof(SaveChickenRequestDto).Name))
            {
                resource = "SaveChickenRequest";
            }
            Console.WriteLine($"Creating GenericDtoService for resource: {resource}");
            return new GenericDtoService<TDto, TSearchDto>(_httpClient, resource);
        }
    }
}
