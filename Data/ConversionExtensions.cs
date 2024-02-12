using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;

namespace LinguacApi.Data
{
    public static class ConversionExtensions
    {
        public static StoryDto ToDto(this Story story) =>
            new(story.Id, story.Title, story.Content, story.Language, story.Level);

        public static QuestionDto ToDto(this Question question) =>
            new(question.Id, question.Text);
    }
}
