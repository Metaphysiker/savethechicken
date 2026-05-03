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
                using var content = new MultipartFormDataContent();

                // Open the stream with your high limit
                // Note: RequestImageFileAsync in the UI already shrunk the file,
                // so this stream is now much smaller and safer.
                var fileStream = file.OpenReadStream(maxAllowedSize: 100 * 1024 * 1024);

                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(streamContent, "file", file.Name);

                var response = await _httpClient.PostAsync("api/aws-files/upload", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                // Log non-success status codes here
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
