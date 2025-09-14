using System.Text.RegularExpressions;

namespace Utilidades.Api.Extensions;

public static partial class StringExtensions {
    /// <summary>
    /// Metodo para normalizar strings
    /// </summary>
    public static string ToInsensitive(this string value) {
        return value.ToLower().Trim().Normalize();
    }

    public static string ToCapitalized(this string value, bool trim = true) {
        var val = value.Split().Select(x => {
            x = x.ToLower();
            if (trim) {
                x = x.Trim();
            }

            x = x[..1].ToUpper() + x[1..];

            return x;
        }).Aggregate((x, y) => $"{x} {y}");
        ;


        return val;
    }

    public static bool IsMail(this string value) {
        value = value.ToInsensitive();

        return MailRegex().IsMatch(value);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]")]
    private static partial Regex MailRegex();
}