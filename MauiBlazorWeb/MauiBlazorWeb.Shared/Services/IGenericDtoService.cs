using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Dtos.DtosImpl;

namespace MauiBlazorWeb.Web.Services
{
    public interface IGenericDtoService<TDto, TSearchDto>
        where TDto : class
        where TSearchDto : class
    {
        Task<TDto> GetByIdAsync(int id);
        Task<List<TDto>> GetAllAsync();
        Task<TDto> CreateAsync(TDto dto);
        Task<TDto> UpdateAsync(TDto dto);
        Task DeleteAsync(int id);
        Task<PaginationDto<TDto>> SearchAsync(TSearchDto searchDto);
    }
}
