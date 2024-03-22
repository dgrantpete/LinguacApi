using System.ComponentModel.DataAnnotations;

namespace LinguacApi.Configurations
{
	public class PromptGenerationConfiguration
	{
		[Required]
		public required string PromptTemplatesPath { get; set; }
	}
}
