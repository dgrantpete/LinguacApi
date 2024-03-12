namespace LinguacApi.Data.Dtos
{
    public record AccountRegistrationDto(string Email, string Password);

    public record LoginDto(string Email, string Password);

    public record ChangePasswordDto(string OldPassword, string NewPassword);
}
