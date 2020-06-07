using System.Text.Json;

namespace Surveillance.App.Json
{
    public static class DefaultJsonOptions
    {
        public static JsonSerializerOptions Instance { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }
}