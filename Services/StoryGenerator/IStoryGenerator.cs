using LinguacApi.Data;

namespace LinguacApi.Services.StoryGenerator
{
    public interface IStoryGenerator
    {
        Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level);

        Task<IEnumerable<string>> GenerateStoryQuestions(string storyContent);
    }
}
