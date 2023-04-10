using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Services.Extensions
{
    public static class ResponseMessageExtensions
    {
        public static async Task<T?> ReadContentAsJsonAsync<T>(this ResponseMessage responseMessage)
        {
            T document = default(T);
            using (StreamReader streamReader = new StreamReader(responseMessage.Content))
            {
                string responseContent = await streamReader.ReadToEndAsync();
                document = JsonConvert.DeserializeObject<T>(responseContent);
            }

            return document;

        }
    }
}
