using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email.Templates.PasswordReset
{
	public record PasswordResetModel(string ResetPasswordLink) : IEmailModel
	{
		public string Subject => "Password Reset Request";

		public EmailAddress Sender => new("accounts@linguac.com", "Linguac Accounts");

		public EmailAddress ReplyTo => new("support@linguac.com", "Linguac Support");
	}
}
