using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;

namespace LinguacApi.Templates.Prompts.GenerateStory
{
	public record GenerateStoryModel(Language Language, CefrLevel Level, StoryResponse SampleResponse)
	{
		public string? CustomPrompt { get; init; }

		public IEnumerable<string>? SeedWords { get; init; }
	}
}
