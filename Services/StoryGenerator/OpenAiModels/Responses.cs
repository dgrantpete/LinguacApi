namespace LinguacApi.Services.StoryGenerator.OpenAiModels
{
	public record StoryResponse(string Title, string Content);

	public record QuestionsResponse(IEnumerable<QuestionResponse> Questions);

	public record QuestionResponse(string Text, string CorrectAnswer, IEnumerable<string> WrongAnswers);
}
