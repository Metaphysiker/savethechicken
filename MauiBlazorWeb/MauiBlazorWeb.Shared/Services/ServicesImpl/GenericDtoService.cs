using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Web.Services.ServicesImpl
{
    public class GenericDtoService<TDto, TSearchDto> : IGenericDtoService<TDto, TSearchDto>
        where TDto : class
        where TSearchDto : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public GenericDtoService(HttpClient httpClient, string resource)
        {
            _httpClient = httpClient;
            _baseUrl = httpClient.BaseAddress + "api/" + resource;
        }

        public async Task<TDto> GetByIdAsync(int id)
        {
            var result = await _httpClient.GetFromJsonAsync<TDto>($"{_baseUrl}/{id}");
            if (result == null) throw new InvalidOperationException($"No DTO found for id {id}");
            return result;
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<List<TDto>>(_baseUrl);
            if (result == null) throw new InvalidOperationException("No DTOs found");
            return result;
        }

        public async Task<TDto> CreateAsync(TDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync(_baseUrl, dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TDto>();
            if (result == null) throw new InvalidOperationException("Failed to create DTO");
            return result;
        }

        public async Task<TDto> UpdateAsync(TDto dto)
        {
            var response = await _httpClient.PutAsJsonAsync(_baseUrl, dto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TDto>();
            if (result == null) throw new InvalidOperationException("Failed to update DTO");
            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<PaginationDto<TDto>> SearchAsync(TSearchDto searchDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/search", searchDto);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<PaginationDto<TDto>>();
            if (result == null) throw new InvalidOperationException("Search returned no results");
            return result;
        }
    }
}
