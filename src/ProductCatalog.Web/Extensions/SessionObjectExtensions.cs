using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ProductCatalog.Web.Extensions;

// Session'a nesne yazıp okumak için yardımcı metotlar.
// Nesne JSON string'e çevrilip session'da saklanır.
public static class SessionObjectExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static void SetObject<T>(this ISession session, string key, T value)
    {
        ArgumentNullException.ThrowIfNull(session);
        session.SetString(key, JsonSerializer.Serialize(value, JsonOptions));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        ArgumentNullException.ThrowIfNull(session);

        var json = session.GetString(key);
        return string.IsNullOrEmpty(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, JsonOptions);
    }
}
