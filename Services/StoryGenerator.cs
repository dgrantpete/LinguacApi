using LinguacApi.Data;

namespace LinguacApi.Services
{
    public class StoryGenerator : IStoryGenerator
    {
        public Task<string> GenerateStoryContent(Language language, CefrLevel level)
        {
            return Task.FromResult("This is a story");
        }
    }
}
