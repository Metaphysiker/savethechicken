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
}
