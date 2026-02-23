using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace MaschinenDataein.Helper
{
    public static class SessionHelper
    {
        public static void SetObjectInSession(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetCustomObjectFromSession<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            if (string.IsNullOrEmpty(value))
                return default;

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}