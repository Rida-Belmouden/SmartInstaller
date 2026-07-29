namespace SmartInstaller.Services.InstallerProfiles.Common;

internal static class InstallerProfileValidation
{
    public static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    public static bool IsValidDownloadUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp ||
               uri.Scheme == Uri.UriSchemeHttps;
    }

    public static bool IsValidSha256(string? value)
    {
        if (value is null)
        {
            return true;
        }

        return value.Length == 64 &&
               value.All(Uri.IsHexDigit);
    }
}
