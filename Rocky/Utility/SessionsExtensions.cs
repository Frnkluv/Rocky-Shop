using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Rocky.Utility
{
    public static class SessionsExtensions
    {
        // Для сохранения сложных объектов путем сериализации и десериализации
        // первый парам должен указать на то свойство которое нужно для этого метода
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // получение объекта после сериализации
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        // после иду в класс WC и добавляю ключ для доступа к сеансу
    }
}
