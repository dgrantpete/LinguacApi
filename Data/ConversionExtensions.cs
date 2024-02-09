using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;

namespace LinguacApi.Data
{
    public static class ConversionExtensions
    {
        public static StoryDto ToDto(this Story story) =>
            new(story.Id, story.Title, story.Content, story.Language, story.Level);

        public static Story ToModel(this StoryDto storyDTO) =>
            new(storyDTO.Id, storyDTO.Title, storyDTO.Content, storyDTO.Language, storyDTO.Level);
    }
}
