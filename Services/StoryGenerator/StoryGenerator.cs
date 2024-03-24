using Flurl.Http;
using Flurl.Http.Configuration;
using LinguacApi.Configurations;
using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using LinguacApi.Services.StoryGenerator.PromptGeneration;
using LinguacApi.Services.StoryGenerator.PromptGeneration.PromptTemplates.GenerateQuestions;
using LinguacApi.Services.StoryGenerator.PromptGeneration.PromptTemplates.GenerateStory;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LinguacApi.Services.StoryGenerator
{
	public class StoryGenerator : IStoryGenerator
	{
		private readonly OpenAiConfiguration openAiConfiguration;

		private readonly JsonSerializerOptions jsonSerializerOptions;

		private readonly DefaultJsonSerializer jsonSerializer;

		private readonly IPromptGenerator promptGenerator;

		public StoryGenerator(
			IOptions<OpenAiConfiguration> openAiConfiguration,
			IOptionsSnapshot<JsonSerializerOptions> serializationOptions,
			IPromptGenerator promptGenerator)
		{
			this.openAiConfiguration = openAiConfiguration.Value;
			jsonSerializerOptions = serializationOptions.Get(SerializerOptions.OpenAiSerializer);
			jsonSerializer = new(jsonSerializerOptions);
			this.promptGenerator = promptGenerator;
		}

		public async Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level, string? prompt = null, IEnumerable<string>? seedWords = null)
		{
			var prompts = await promptGenerator.GeneratePrompt(new GenerateStoryModel
			(
				language,
				level,
				new("Sample Title", "Paragraph 1...\n\nParagraph 2...")
			)
			{ CustomPrompt = prompt, SeedWords = seedWords });

			JsonObject response = await openAiConfiguration.EndpointUrl
				.WithSettings(settings => settings.JsonSerializer = jsonSerializer)
				.WithOAuthBearerToken(openAiConfiguration.ApiKey)
				.PostJsonAsync(new CompletionRequest(
					openAiConfiguration.Model,
					4095,
					1.1,
					prompts,
					new ResponseFormat("json_object")
				)).ReceiveJson<JsonObject>();

			string storyResponseJson = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
				?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

			return JsonSerializer.Deserialize<StoryResponse>(storyResponseJson, jsonSerializerOptions)
				?? throw new StoryGenerationException("The model did not follow the expected JSON structure.");
		}

		public async Task<IEnumerable<QuestionResponse>> GenerateStoryQuestions(string storyContent, Language language, CefrLevel level)
		{
			var prompts = await promptGenerator.GeneratePrompt(new GenerateQuestionsModel(storyContent, language, level));

			JsonObject response = await openAiConfiguration.EndpointUrl
				.WithSettings(settings => settings.JsonSerializer = jsonSerializer)
				.WithOAuthBearerToken(openAiConfiguration.ApiKey)
				.PostJsonAsync(new CompletionRequest(
					openAiConfiguration.Model,
					4095,
					0.9,
					prompts,
					new ResponseFormat("json_object")
				)).ReceiveJson<JsonObject>();

			string questionsResponseJson = response["choices"]?[0]?["message"]?["content"]?.GetValue<string>()
				?? throw new StoryGenerationException("The response from the OpenAI API did not contain the expected JSON structure.");

			return JsonSerializer.Deserialize<QuestionsResponse>(questionsResponseJson, jsonSerializerOptions)?.Questions
				?? throw new StoryGenerationException("The model did not follow the expected JSON structure.");
		}
	}
}
