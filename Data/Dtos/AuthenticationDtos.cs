using System.ComponentModel.DataAnnotations;

namespace LinguacApi.Data.Dtos
{
	public record RegistrationDto
	(
		[EmailAddress] string Email,
		[Length(8, 255)] string Password
	);

	public record LoginDto(string Email, string Password);

	public record ChangePasswordDto
	(
		string OldPassword,
		[Length(8, 255)] string NewPassword
	);

	public record EmailConfirmationDto(Guid ConfirmationId);
}
