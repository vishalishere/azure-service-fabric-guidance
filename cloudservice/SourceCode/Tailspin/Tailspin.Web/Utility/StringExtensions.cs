namespace Tailspin.Web.Utility
{
    public static class StringExtensions
    {
        public static string Capitalize(this string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return word;
            }

            return word[0].ToString().ToUpperInvariant() + word.Substring(1);
        }
    }
}
