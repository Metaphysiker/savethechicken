public interface IDtoConverter<T, DTO>
{
    T Convert(DTO dto);
    DTO Convert(T model);
    List<T> Convert(List<DTO> dtos);
    List<DTO> Convert(List<T> models);
}
