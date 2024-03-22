using LinguacApi.Data;
using LinguacApi.Services.StoryGenerator.OpenAiModels;

namespace LinguacApi.Services.StoryGenerator
{
	public interface IStoryGenerator
	{
		Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level, string? prompt = null, IEnumerable<string>? seedWords = null);

		Task<IEnumerable<QuestionResponse>> GenerateStoryQuestions(string storyContent, Language language, CefrLevel level);
	}
}
