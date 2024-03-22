using LinguacApi.Configurations;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using Microsoft.Extensions.Options;
using RazorLight;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration
{
	public class PromptGenerator(IRazorLightEngine razorEngine, IOptions<PromptGenerationConfiguration> promptConfiguration) : IPromptGenerator
	{
		private readonly PromptGenerationConfiguration _promptConfiguration = promptConfiguration.Value;

		private readonly Dictionary<string, IList<PromptTemplateInfo>> _templateInfoCache = [];

		public async Task<IList<Message>> GeneratePrompt<TModel>(TModel model, string? templateName = default)
		{
			templateName ??= typeof(TModel)?.Namespace?.Split('.').LastOrDefault()
				?? throw new InvalidOperationException($"Error getting template name for '{typeof(TModel).Name}'. Ensure model is located in correct namespace to allow for implicit name derivation, or specify it explicitly.");

			var templateInfos = GetPromptTemplateInfos(templateName);

			// await is not redundant here, removal and returning the task directly doesn't allow the
			// Task<Message[]> to implicitly cast to Task<IList<Message>>
			return await Task.WhenAll(templateInfos.Select(async templateInfo =>
			{
				string promptContent = await razorEngine.CompileRenderAsync(Path.Combine("Prompts", templateInfo.TemplateRelativePath), model);
				return new Message(templateInfo.Role, promptContent);
			}));
		}

		private IList<PromptTemplateInfo> GetPromptTemplateInfos(string templateName)
		{
			if (_templateInfoCache.TryGetValue(templateName, out var paths))
			{
				return paths;
			}

			string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _promptConfiguration.PromptTemplatesPath, templateName);

			IList<PromptTemplateInfo> templateInfos = Directory.EnumerateFiles(basePath, "*.cshtml", SearchOption.AllDirectories)
				.Select(fullPath => Path.GetFileNameWithoutExtension(fullPath))
				.Select(fileName => new { FileName = fileName, NameParts = fileName.Split('.') })
				.Select(x => new
				{
					PromptTemplateInfo = new PromptTemplateInfo
					(
						Path.Combine(templateName, x.FileName),
						Enum.Parse<MessageRole>(x.NameParts.Last())
					),
					Order = int.Parse(x.NameParts.First())
				})
				.OrderBy(x => x.Order)
				.Select(x => x.PromptTemplateInfo)
				.ToList();

			_templateInfoCache[templateName] = templateInfos;

			return templateInfos;
		}

		private record struct PromptTemplateInfo(string TemplateRelativePath, MessageRole Role);
	}
}
