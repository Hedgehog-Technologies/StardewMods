namespace AutoForager.Extensions
{
    public static class StringExtensions
    {
        public static bool IEquals(this string a, string b)
        {
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}
