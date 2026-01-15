using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Helpers
{
    public static class SessionJson
    {
        public static void Set<T>(ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(ISession session, string key)
        {
            var s = session.GetString(key);
            if (string.IsNullOrWhiteSpace(s)) return default;
            return JsonSerializer.Deserialize<T>(s);
        }
    }
}
