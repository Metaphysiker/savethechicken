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
            try
            {
                // Buffer into MemoryStream first so the upload doesn't depend on the browser's
                // live file handle — prevents iOS Safari from dropping the stream mid-upload.
                using var memoryStream = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024).CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(memoryStream);
                var contentType = string.IsNullOrEmpty(file.ContentType) ? "application/octet-stream" : file.ContentType;
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                content.Add(streamContent, "file", file.Name);

                var response = await _httpClient.PostAsync("api/aws-files/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                var error = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"Upload failed with status code {response.StatusCode}: {error}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Upload Exception: {ex.Message}");
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
