using Shared.Dtos.DtosImpl;
using System.Text.RegularExpressions;
using WebApi.Factories.FactoriesImpl;
using WebApi.Models.ModelsImpl;

namespace WebApi.Services.ServicesImpl
{
    public class BlackListDetectorService
    {
        public List<Person> CheckIfEntityMatchesBlackListedPersons(List<Person> blackListedPersons, Address address, Contact contact)
        {
            return blackListedPersons
                .Where(p =>
                    (p.Address != null && address != null && AddressMatches(address, p.Address))
                    || (p.Contact != null && contact != null && ContactMatches(contact, p.Contact))
                )
                .ToList();
        }

        private string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            value = value.ToLowerInvariant();
            value = Regex.Replace(value, @"[^\w\s]", ""); // remove punctuation
            value = Regex.Replace(value, @"\s+", " ").Trim(); // normalize whitespace

            return value;
        }

        private int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
            if (string.IsNullOrEmpty(t)) return s.Length;

            var d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost
                    );
                }
            }

            return d[s.Length, t.Length];
        }

        private double Similarity(string a, string b)
        {
            a = Normalize(a);
            b = Normalize(b);

            int distance = LevenshteinDistance(a, b);
            int maxLen = Math.Max(a.Length, b.Length);

            return maxLen == 0 ? 1.0 : 1.0 - (double)distance / maxLen;
        }

        private bool AddressMatches(Address a, Address b)
        {
            const double threshold = 0.8;

            double streetScore = Similarity(a.Street, b.Street);
            double cityScore = Similarity(a.City, b.City);

            bool postalMatch = Normalize(a.PostalCode) == Normalize(b.PostalCode);

            return postalMatch && streetScore >= threshold && cityScore >= threshold;
        }

        private bool ContactMatches(Contact a, Contact b)
        {
            const double nameThreshold = 0.85;

            double firstNameScore = Similarity(a.FirstName, b.FirstName);
            double lastNameScore = Similarity(a.LastName, b.LastName);

            bool emailMatch = !string.IsNullOrEmpty(a.Email)
                && Normalize(a.Email) == Normalize(b.Email);

            bool phoneMatch = !string.IsNullOrEmpty(a.PhoneNumber)
                && Normalize(a.PhoneNumber) == Normalize(b.PhoneNumber);

            return
                (firstNameScore >= nameThreshold && lastNameScore >= nameThreshold)
                || emailMatch
                || phoneMatch;
        }


    }
}
