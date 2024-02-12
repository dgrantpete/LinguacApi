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
        private const string SystemStoryPrompt = """
            You write short stories, given a language and a CEFR level, to assist people in learning new languages through consuming content. These stories must only be spoken in the language requested. You respond in a JSON format, which has a "title" and a "content" property, like this:
            {
               "title": "The Title of the Story",
               "content": "First paragraph.\n\nSecond paragraph..."
            }

            You create interesting stories with a cohesive plot, characters, and setting(s) of your choosing, modulating the comprehension difficulty to what is appropriate for someone of the provided CEFR level.
            """;

        private const string SystemQuestionPrompt = """
            You write open-ended questions aimed at helping language learners evaluate their comprehension of a given input text.

            When a user provides you with an input text, write 5-10 open-response questions that test the reader's comprehension of that text. These questions should, ideally, require more than just a 1 word answer, testing things like:
            Comprehension of grammar rules (i.e. why a verb was conjugated a certain way, why a noun or an adjective was masculine or feminine).
            Understanding the significance of words (i.e. asking the reader to write their own sentence using a given word or asking for a rough definition of a word).
            General comprehension of the text, ensuring the content is being understood.

            Ensure that there is a variety of questions from all 3 of these categories.

            The questions must be in the same language as the provided text. Each question should be separated by 2 newlines.
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
                    new List<Message>
                    {
                        new(MessageRole.System, SystemStoryPrompt),
                        new(MessageRole.User, $"Write me a story in {language} with a difficulty of {level}.")
                    },
                    _random.Next(), // Using a random seed to promote diversity in the generated stories
                    new ResponseFormat("json_object")
                )).ReceiveJson<JsonObject>();

            string storyResponseJson = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
                ?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

            return JsonSerializer.Deserialize<StoryResponse>(storyResponseJson, OpenAiSerializationOptions.Instance)
                ?? throw new StoryGenerationException("The model did not follow the expected JSON structure.");
        }

        public async Task<IEnumerable<string>> GenerateStoryQuestions(string storyContent)
        {
            JsonObject response = await openAiConfiguration.EndpointUrl
                .WithSettings(settings => settings.JsonSerializer = _jsonSerializer)
                .WithOAuthBearerToken(openAiConfiguration.ApiKey)
                .PostJsonAsync(new CompletionRequest(
                    openAiConfiguration.Model,
                    2048,
                    1.1,
                    new List<Message>
                    {
                        new(MessageRole.System, SystemQuestionPrompt),
                        new(MessageRole.User, storyContent)
                    }
                )).ReceiveJson<JsonObject>();

            string questions = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
                ?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

            return questions.Split("\n\n");
        }
    }
}
