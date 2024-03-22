namespace LinguacApi.Services.Authentication.JwtHandler
{
	public record TokenResult(string Value, DateTime Expiration);
}
