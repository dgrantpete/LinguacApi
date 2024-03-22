using LinguacApi.Services.Email;
using SendGrid.Helpers.Mail;

namespace LinguacApi.Templates.Emails.ConfirmEmail
{
	public record ConfirmEmailModel(string ConfirmationLink) : IEmailModel
	{
		public string Subject => "Verify your Email Address";

		public EmailAddress Sender => new("accounts@linguac.com", "Linguac Accounts");

		public EmailAddress ReplyTo => new("support@linguac.com", "Linguac Support");
	}
}
