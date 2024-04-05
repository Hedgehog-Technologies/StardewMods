using StardewModdingAPI;

namespace AutoForager.Extensions
{
    public static class SemanticVersionExtensions
    {
        public static bool IsEqualToOrNewerThan(this ISemanticVersion current, ISemanticVersion other)
        {
            return current.Equals(other)
                || current.IsNewerThan(other);
        }

        public static bool IsEqualToOrNewerThan(this ISemanticVersion current, string other)
        {
            return current.IsEqualToOrNewerThan(new SemanticVersion(other));
        }
    }
}
