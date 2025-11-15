using System.Text;

namespace GeoAuth.Shared.Extensions;

public static class StringExtensions
{
    public static string Base64Encode(this string value, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        return Convert.ToBase64String(encoding.GetBytes(value));
    }
}
