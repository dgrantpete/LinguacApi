using LinguacApi.Data;

namespace LinguacApi.Services
{
    public interface IStoryGenerator
    {
        Task<string> GenerateStoryContent(Language language, CefrLevel level);
    }
}
