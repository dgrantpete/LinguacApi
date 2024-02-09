using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinguacApi.Services.StoryGenerator
{
    public static class OpenAiSerializationOptions
    {
        private static JsonSerializerOptions? _instance;

        public static JsonSerializerOptions Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        PropertyNameCaseInsensitive = true
                    };

                    _instance.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
                }

                return _instance;
            }
        }
    }
}
