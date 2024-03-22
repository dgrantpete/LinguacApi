using LinguacApi.Data;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration.PromptTemplates.GenerateQuestions
{
	public record GenerateQuestionsModel(string StoryContent, Language Language, CefrLevel Level);
}
