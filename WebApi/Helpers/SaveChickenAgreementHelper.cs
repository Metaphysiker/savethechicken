using WebApi.Models.ModelsImpl;

public static class SaveChickenAgreementHelper
{
    public static DateTime GetAgreementDate(IEnumerable<DateOnly> dates)
    {
        if (dates == null || !dates.Any())
            return DateTime.Now;

        var today = DateOnly.FromDateTime(DateTime.Today);

        var selectedDate = dates
            .OrderBy(d => d)
            .FirstOrDefault(d => d >= today);

        if (selectedDate == default)
        {
            selectedDate = dates.Max();
        }

        return selectedDate.ToDateTime(TimeOnly.MinValue);
    }

    public static string GetOvernehmerName(SaveChickenRequest saveChickenRequest)
    {
        var contact = saveChickenRequest?.Person?.Contact;
        var address = saveChickenRequest?.Person?.Address;

        var fullName = string.Join(" ",
            new[] { contact?.FirstName, contact?.LastName }
                .Where(s => !string.IsNullOrWhiteSpace(s))
        );

        var street = address?.Street;

        var cityPostal = string.Join(" ",
            new[] { address?.PostalCode, address?.City }
                .Where(s => !string.IsNullOrWhiteSpace(s))
        );

        return string.Join(", ",
            new[] { fullName, street, cityPostal }
                .Where(s => !string.IsNullOrWhiteSpace(s))
        );
    }
}
