using RazorLight;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinguacApi.Services.StoryGenerator.PromptGeneration
{
	public abstract class PromptTemplate<TModel> : TemplatePage<TModel>
	{
		private readonly JsonSerializerOptions jsonSerializerOptions = new()
		{
			WriteIndented = true,
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
		};

		public PromptTemplate()
		{
			jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
		}

		public string SerializeExample<T>(T example) => JsonSerializer.Serialize(example, jsonSerializerOptions);

		public override void Write(string value)
		{
			DisableEncoding = true;
			base.Write(value);
		}
	}
}
