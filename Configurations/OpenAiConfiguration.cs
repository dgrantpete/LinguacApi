using System.ComponentModel.DataAnnotations;

namespace LinguacApi.Configurations
{
	public class OpenAiConfiguration
	{
		[Required]
		public required string ApiKey { get; set; }

		[Required]
		public required string Model { get; set; }

		[Required]
		public required string EndpointUrl { get; set; }
	}
}
