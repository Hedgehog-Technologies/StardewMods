namespace HedgeTech.Common.Extensions
{
	public static class StringExtensions
	{
		public static bool IEquals(this string a, string b)
		{
			if (a is null || b is null) return false;

			return string.Equals(a, b, System.StringComparison.InvariantCultureIgnoreCase);
		}

		public static bool IsNullOrEmpty(this string? str)
		{
			return string.IsNullOrEmpty(str);
		}
	}
}
