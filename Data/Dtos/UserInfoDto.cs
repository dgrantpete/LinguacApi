namespace LinguacApi.Data.Dtos
{
	public record UserInfoDto(IEnumerable<string> Roles)
	{
		public string? Email { get; init; }
	}
}
