using Flurl.Http;
using Flurl.Http.Configuration;
using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LinguacApi.Services.StoryGenerator
{
    public class StoryGenerator(IOptions<OpenAiConfiguration> openAiConfiguration, IOptions<PromptConfiguration> promptConfiguration) : IStoryGenerator
    {
        private readonly OpenAiConfiguration _openAiConfiguration = openAiConfiguration.Value;

        private readonly PromptConfiguration _promptConfiguration = promptConfiguration.Value;

        private static readonly Random _random = new();

        private static readonly DefaultJsonSerializer _jsonSerializer = new(OpenAiSerializationOptions.Instance);

        public async Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level, string? prompt = null)
        {
            string userPrompt = $"Write me a story in {language} with a difficulty of {level}" + (prompt is null ? "." : " based on the following:\n" + prompt);

            JsonObject response = await _openAiConfiguration.EndpointUrl
                .WithSettings(settings => settings.JsonSerializer = _jsonSerializer)
                .WithOAuthBearerToken(_openAiConfiguration.ApiKey)
                .PostJsonAsync(new CompletionRequest(
                    _openAiConfiguration.Model,
                    4095,
                    1.1,
                    new List<Message>
                    {
                        new(MessageRole.System, _promptConfiguration.SystemStoryPrompt),
                        new(MessageRole.User, userPrompt)
                    },
                    _random.Next(), // Using a random seed to promote diversity in the generated stories
                    new ResponseFormat("json_object")
                )).ReceiveJson<JsonObject>();

            string storyResponseJson = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
                ?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

            return JsonSerializer.Deserialize<StoryResponse>(storyResponseJson, OpenAiSerializationOptions.Instance)
                ?? throw new StoryGenerationException("The model did not follow the expected JSON structure.");
        }

        public async Task<IEnumerable<QuestionResponse>> GenerateStoryQuestions(string storyContent, Language language, CefrLevel level)
        {
            JsonObject response = await _openAiConfiguration.EndpointUrl
                .WithSettings(settings => settings.JsonSerializer = _jsonSerializer)
                .WithOAuthBearerToken(_openAiConfiguration.ApiKey)
                .PostJsonAsync(new CompletionRequest(
                    _openAiConfiguration.Model,
                    4095,
                    0.9,
                    new List<Message>
                    {
                        new(MessageRole.System, string.Format(_promptConfiguration.SystemQuestionPrompt, level, language)),
                        new(MessageRole.User, storyContent)
                    },
                    _random.Next(), // Using a random seed to promote diversity in the generated stories
                    new ResponseFormat("json_object")
                )).ReceiveJson<JsonObject>();

            string questionsResponseJson = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
                ?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

            return JsonSerializer.Deserialize<QuestionsResponse>(questionsResponseJson, OpenAiSerializationOptions.Instance)?.Questions
                ?? throw new StoryGenerationException("The model did not follow the expected JSON structure.");
        }
    }
}
