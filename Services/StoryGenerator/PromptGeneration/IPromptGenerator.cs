using LinguacApi.Services.StoryGenerator.OpenAiModels;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration
{
	public interface IPromptGenerator
	{
		public Task<IList<Message>> GeneratePrompt<TModel>(TModel model, string? templateName = default);
	}
}