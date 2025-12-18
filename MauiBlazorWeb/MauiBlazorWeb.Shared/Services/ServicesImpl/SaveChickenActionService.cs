using MauiBlazorWeb.Shared.Factories.FactoriesImpl;
using MauiBlazorWeb.Web.Services.ServicesImpl;
using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Shared.Services.ServicesImpl
{
    public class SaveChickenActionService
    {
        private readonly GenericDtoService<SaveChickenActionDto, SaveChickenActionSearch> _saveChickenActionService;
        public SaveChickenActionService(GenericDtoServiceFactory genericDtoServiceFactory)
        {
            _saveChickenActionService = genericDtoServiceFactory.Create<SaveChickenActionDto, SaveChickenActionSearch>();
        }

        public async Task<SaveChickenActionDto?> GetActiveSaveChickenAction()
        {
            SaveChickenActionSearch search = new SaveChickenActionSearch
            {
                IsActive = true,
                PageSize = 1
            };

            var searchResults = await _saveChickenActionService.SearchAsync(search);
            return searchResults.Data.FirstOrDefault();
        }

    }
}
