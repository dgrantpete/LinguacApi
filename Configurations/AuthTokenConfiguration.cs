using System.ComponentModel.DataAnnotations;

namespace LinguacApi.Configurations
{
	public class AuthTokenConfiguration
	{
		[Required]
		public required string AccessSecret { get; set; }

		[Required]
		public required string RefreshSecret { get; set; }

		[Required]
		public required string Issuer { get; set; }

		[Required]
		public required string Audience { get; set; }

		[Required]
		public required TimeSpan AccessExpirationTime { get; set; }

		[Required]
		public required TimeSpan RefreshExpirationTime { get; set; }

		[Required]
		public required string AccessCookieName { get; set; }

		[Required]
		public required string RefreshCookieName { get; set; }

		[Required]
		public required string AccessCookieDomain { get; set; }

		[Required]
		public required string RefreshCookieDomain { get; set; }
	}
}
