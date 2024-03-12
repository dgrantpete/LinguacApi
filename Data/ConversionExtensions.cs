using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;

namespace LinguacApi.Data
{
    public static class ConversionExtensions
    {
        public static StoryDto ToDto(this Story story) =>
            new(story.Id, story.Title, story.Content, story.Language, story.Level);

        public static StorySummaryDto ToSummaryDto(this Story story) =>
            new(story.Id, story.Title, story.Language, story.Level);

        public static QuestionDto ToDto(this Question question) =>
            new(question.Id, question.Text, question.Answers.Select(answer => answer.ToDto()));

        public static AnswerDto ToDto(this Answer answer) =>
            new(answer.Id, answer.Text);
    }
}
