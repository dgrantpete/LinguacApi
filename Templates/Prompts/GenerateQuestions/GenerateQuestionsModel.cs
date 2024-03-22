using LinguacApi.Data;

namespace LinguacApi.Templates.Prompts.GenerateQuestions
{
	public record GenerateQuestionsModel(string StoryContent, Language Language, CefrLevel Level);
}
