using System.ComponentModel.DataAnnotations;

namespace LinguacApi.Configurations
{
	public class EmailConfiguration
	{
		[Required]
		public required string ApiKey { get; set; }

		[Required]
		public required string ConfirmationUrl { get; set; }

		[Required]
		public required TimeSpan ConfirmationExpirationTime { get; set; }

		[Required]
		public required string ResetPasswordUrl { get; set; }

		[Required]
		public required TimeSpan ResetPasswordExpirationTime { get; set; }

		[Required]
		public required string EmailTemplatesPath { get; set; }
	}
}
