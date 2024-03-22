using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration.PromptTemplates.GenerateStory
{
	public record GenerateStoryModel(Language Language, CefrLevel Level, StoryResponse SampleResponse)
	{
		public string? CustomPrompt { get; init; }

		public IEnumerable<string>? SeedWords { get; init; }
	}
}
