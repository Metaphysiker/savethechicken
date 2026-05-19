using Shared.Dtos.DtosImpl;

namespace Shared.Extensions
{
    public static class SaveChickenRequestDtoExtensions
    {
        /// <summary>
        /// Returns a copy with only scalar properties — no Files, Person, or SaveChickenAction navigation properties.
        /// Use this before PUT calls to avoid sending large payloads.
        /// </summary>
        public static SaveChickenRequestDto ToUpdatePayload(this SaveChickenRequestDto dto) => new()
        {
            Id = dto.Id,
            PersonId = dto.PersonId,
            NumberOfChickensToBeSaved = dto.NumberOfChickensToBeSaved,
            NumberOfRoostersToBeSaved = dto.NumberOfRoostersToBeSaved,
            DescriptionOfPlaceForChickens = dto.DescriptionOfPlaceForChickens,
            AcceptTermsAndConditions = dto.AcceptTermsAndConditions,
            ConfirmThatIFulfillCriteria = dto.ConfirmThatIFulfillCriteria,
            Message = dto.Message,
            SaveChickenActionId = dto.SaveChickenActionId,
            NumberOfBoxes = dto.NumberOfBoxes,
            Color = dto.Color,
            IsSubmittedFromPublicForm = dto.IsSubmittedFromPublicForm,
            IsHandled = dto.IsHandled,
            GenericName = dto.GenericName,
        };
    }
}
