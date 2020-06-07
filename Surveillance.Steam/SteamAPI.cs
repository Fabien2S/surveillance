using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Surveillance.Steam.Response;

namespace Surveillance.Steam
{
    public static class SteamApi
    {
        public static string Key => _key ?? throw new InvalidOperationException("Key not set");
        
        private static readonly HttpClient HttpClient = new HttpClient();
        private static string _key;
        
        public static void Init(string key)
        {
            _key = key;
        }

        public static void Reset()
        {
            _key = null;
        }

        public static async Task<SteamApiResponse<T>> Request<T>(Uri uri, JsonSerializerOptions options) where T : struct
        {
            var responseMessage = await HttpClient.GetAsync(uri);
            var responseMessageContent = responseMessage.Content;
            
            var responseStream = await responseMessageContent.ReadAsStreamAsync();
            var responseContent = await JsonSerializer.DeserializeAsync<T>(responseStream, options);

            var headers = responseMessageContent.Headers;
            return new SteamApiResponse<T>
            {
                Expires = headers.Expires ?? DateTimeOffset.UtcNow,
                Content = responseContent
            };
        }
    }
}