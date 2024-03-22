using LinguacApi.Data;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration
{
	public static class LanguageSpecificMapping
	{
		public static string GetStoryUserPrompt(this Language language)
		{
			return language switch
			{
				Language.English => "Write me a story in English, please",
				Language.Spanish => "Scríbeme una historia en español, por favor",
				Language.French => "Écrivez-moi une histoire en français, s'il vous plaît",
				Language.German => "Schreiben Sie mir bitte eine Geschichte auf Deutsch",
				Language.Italian => "Scrivimi una storia in italiano, per favore",
				_ => $"Write me a story in {language}, please"
			};
		}
	}
}
