using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MauiBlazorWeb.Shared.Services.ServicesImpl
{
    public class AwsFileService
    {
        private readonly HttpClient _httpClient;
        public AwsFileService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string?> UploadFileAsync(IBrowserFile file)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 100)); // 100MB max
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            var response = await _httpClient.PostAsync("api/aws-files/upload", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        public async Task<string?> GetPresignedUrlByKeyAsync(string key)
        {
            var response = await _httpClient.GetAsync($"api/aws-files/get-presigned-url-by-key?key={Uri.EscapeDataString(key)}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
    }
}