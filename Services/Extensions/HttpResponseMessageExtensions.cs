using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Services.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<T?> ReadContentAsJsonWithOptionAsync<T>(this HttpResponseMessage message)
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return await message.Content.ReadFromJsonAsync<T>(jsonSerializerOptions);
        }
    }
}
