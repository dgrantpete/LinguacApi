namespace LinguacApi.Data.Dtos
{
	public record TokenStatusDto(DateTime AccessTokenExpiration, DateTime RefreshTokenExpiration);

	public record AccessTokenStatusDto(DateTime AccessTokenExpiration);
}
