using Flurl.Http;
using Flurl.Http.Configuration;
using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LinguacApi.Services.StoryGenerator
{
    public class StoryGenerator(OpenAiConfiguration openAiConfiguration) : IStoryGenerator
    {
        private const string SystemPrompt = """
            You write short stories, given a language and a CEFR level, to assist people in learning new languages through consuming content. These stories must only be spoken in the language requested. You respond in a JSON format, which has a "title" and a "content" property, like this:
            {
               "title": "The Title of the Story",
               "content": "First paragraph.\n\nSecond paragraph..."
            }

            You create interesting stories with a cohesive plot, characters, and setting(s) of your choosing, modulating the comprehension difficulty to what is appropriate for someone of the provided CEFR level.
            """;

        private static readonly Random _random = new();

        private static readonly DefaultJsonSerializer _jsonSerializer = new(OpenAiSerializationOptions.Instance);

        public async Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level)
        {
            JsonObject response = await openAiConfiguration.EndpointUrl
                .WithSettings(settings => settings.JsonSerializer = _jsonSerializer)
                .WithOAuthBearerToken(openAiConfiguration.ApiKey)
                .PostJsonAsync(new CompletionRequest(
                    openAiConfiguration.Model,
                    2048,
                    1.1,
                    _random.Next(), // Using a random seed to promote diversity in the generated stories
                    new ResponseFormat("json_object"),
                    new List<Message>
                    {
                        new(MessageRole.System, SystemPrompt),
                        new(MessageRole.User, $"Write me a story in {language} with a difficulty of {level}.")
                    }
                )).ReceiveJson<JsonObject>();

            return response["choices"]?[0]?["message"]?["content"]
               .Deserialize<StoryResponse>(OpenAiSerializationOptions.Instance)
                ?? throw new StoryGenerationException("The response from the OpenAI API was not in the expected format.");
        }
    }
}
