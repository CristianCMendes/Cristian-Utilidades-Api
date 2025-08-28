using System.Text.RegularExpressions;

namespace Utilidades.Api.Extensions;

public static partial class StringExtensions {
    /// <summary>
    /// Metodo para normalizar strings
    /// </summary>
    public static string AsInsensitive(this string value) {
        return value.ToLower().Trim().Normalize();
    }

    public static bool IsMail(this string value) {
        value = value.AsInsensitive();

        return MailRegex().IsMatch(value);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]")]
    private static partial Regex MailRegex();
}