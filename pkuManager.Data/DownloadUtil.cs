using System.Text.Json;

namespace pkuManager.Data;

/// <summary>
/// A utility class for downloading files from URLs.
/// </summary>
public static class DownloadUtil
{
    private static readonly JsonDocumentOptions JDO = new()
    {
        AllowTrailingCommas = true,
        //PropertyNameCaseSensitive = true, //by default
        //MaxDepth = 64, //by default
    };

    /// <summary>
    /// Asynchronously downloads the UTF-8 encoded JSON file at <paramref name="url"/>.
    /// </summary>
    /// <param name="url">A URL to a UTF-8 encoded JSON file.</param>
    /// <returns>A <see cref="JsonDocument"/> object representing the downloaded JSON file.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="url"/> is invalid.</exception>
    /// <exception cref="HttpRequestException">A network error occured.</exception>
    /// <exception cref="TaskCanceledException">The HTTP request timed out (set to 30s).</exception>
    /// <exception cref="JsonException"><paramref name="url"/> does not point to a valid UTF-8 encoded JSON file.</exception>
    public static async Task<JsonDocument> DownloadJSON(string url)
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(30); //30 second timeout
        using var stream = await client.GetStreamAsync(url);
        return await JsonDocument.ParseAsync(stream, JDO); //ArgumentException cannot occur
    }
}