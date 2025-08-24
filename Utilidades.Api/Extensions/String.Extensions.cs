namespace Utilidades.Api.Extensions;

public static class StringExtensions {
    /// <summary>
    /// Metodo para normalizar strings
    /// </summary>
    public static string AsInsensitive(this string value) {
        return value.ToLower().Trim().Normalize();
    }
}