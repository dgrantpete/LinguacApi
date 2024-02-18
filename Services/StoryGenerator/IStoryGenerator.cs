using LinguacApi.Data;

namespace LinguacApi.Services.StoryGenerator
{
    public interface IStoryGenerator
    {
        Task<StoryResponse> GenerateStoryContent(Language language, CefrLevel level, string? prompt = null);

        Task<IEnumerable<string>> GenerateStoryQuestions(string storyContent, Language language, CefrLevel level);
    }
}
