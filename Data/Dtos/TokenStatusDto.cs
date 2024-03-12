namespace LinguacApi.Data.Dtos
{
    public record TokenStatusDto
    {
        public DateTime? AccessTokenExpiration { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }
    }
}
