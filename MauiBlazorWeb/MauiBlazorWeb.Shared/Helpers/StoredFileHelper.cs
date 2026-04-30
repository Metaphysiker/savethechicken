using Microsoft.Extensions.Logging;
using Shared.Dtos;
using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Shared.Helpers
{
    using Microsoft.AspNetCore.Components.Forms;
    using MauiBlazorWeb.Shared.Services.ServicesImpl;
    using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MauiBlazorWeb.Web.Services.ServicesImpl;

    public class StoredFileHelper
    {
        private readonly ILogger<StoredFileHelper> _logger;

        public StoredFileHelper(ILogger<StoredFileHelper> logger)
        {
            _logger = logger;
        }

        public async Task<List<StoredFileDto>> UploadFilesAsync(List<IBrowserFile> files, IEntityWithFileDtos model, AwsFileService awsFileService, GenericDtoService<StoredFileDto, ISearchDto>? fileService = null)
        {
            List<StoredFileDto> storedFiles = new List<StoredFileDto>();
            _logger.LogWarning("DEBUG: Entered UploadFilesAsync, files.Count={FileCount}, modelType={ModelType}", files.Count, model?.GetType().Name);
            foreach (var file in files)
            {
                _logger.LogWarning("DEBUG: About to upload file: {FileName}", file.Name);
                var key = await awsFileService.UploadFileAsync(file);
                _logger.LogWarning("DEBUG: Upload result for {FileName}: {Key}", file.Name, key);
                if(!string.IsNullOrEmpty(key))
                {
                    _logger.LogWarning("DEBUG: Upload succeeded for {FileName}, creating StoredFileDto", file.Name);
                    StoredFileDto storedFile = new StoredFileDto
                    {
                        FileName = file.Name,
                        FileKey = key,
                        ContentType = file.ContentType,
                    };

                    _logger.LogWarning("DEBUG: About to assign model ID to stored file: {FileName}", file.Name);
                    this.AssignModelIdToStoredFile(model, storedFile);
                    _logger.LogWarning("DEBUG: After assigning model ID: {Json}", System.Text.Json.JsonSerializer.Serialize(storedFile));
                    if (fileService != null)
                    {
                        try
                        {
                            _logger.LogWarning("DEBUG: About to call fileService.CreateAsync for: {FileName}", file.Name);
                            await fileService.CreateAsync(storedFile);
                            _logger.LogWarning("DEBUG: Stored file metadata in DB for: {FileName}", file.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "DEBUG: Error saving file metadata for {FileName}", file.Name);
                        }
                    }
                    storedFiles.Add(storedFile);
                    _logger.LogWarning("DEBUG: Added storedFile to storedFiles list: {FileName}", file.Name);
                }
                else
                {
                    _logger.LogWarning("DEBUG: Upload failed or returned empty key for file: {FileName}", file.Name);
                }
            }
            _logger.LogWarning("DEBUG: UploadFilesAsync completed. Stored files count: {Count}", storedFiles.Count);
            return storedFiles;
        }
            public void AssignModelIdToStoredFile(IEntityWithFileDtos? model, StoredFileDto storedFile)
        {
            switch (model)
            {
                case SaveChickenDriveRequestDto saveChickenDriveRequest:
                    storedFile.SaveChickenDriveRequestId = saveChickenDriveRequest.Id;
                    break;
                case SaveChickenRequestDto saveChickenRequest:
                    storedFile.SaveChickenRequestId = saveChickenRequest.Id;
                    break;
                case FarmDto farm:
                    storedFile.FarmId = farm.Id;
                    break;
            }
        }
    }
}
