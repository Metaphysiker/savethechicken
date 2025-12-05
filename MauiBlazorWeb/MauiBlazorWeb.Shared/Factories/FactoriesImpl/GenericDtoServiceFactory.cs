using System;
using System.Net.Http;
using MauiBlazorWeb.Web.Services.ServicesImpl;

namespace MauiBlazorWeb.Shared.Factories.FactoriesImpl
{
    public class GenericDtoServiceFactory
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public GenericDtoServiceFactory(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
        }

        public GenericDtoService<TDto, TSearchDto> Create<TDto, TSearchDto>()
            where TDto : class
            where TSearchDto : class
        {
            return new GenericDtoService<TDto, TSearchDto>(_httpClient, _baseUrl);
        }
    }
}
