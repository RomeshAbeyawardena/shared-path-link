using System.Text;

namespace geo_auth.Extensions;

internal static class StringExtensions
{
    public static string ToFixedLength(this string value, int length)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return new string(' ', length);
        }

        if (value.Length <= length)
        {
            var whitespaceLength = length - value.Length;
            return $"{value}{new string(' ', whitespaceLength)}";
        }

        return string.Concat(value.AsSpan(0, length - 3), "...");
    }

    public static string Base64Encode(this string value, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return Convert.ToBase64String(encoding.GetBytes(value));
    }
}
