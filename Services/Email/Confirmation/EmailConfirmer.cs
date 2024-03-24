using Flurl;
using LinguacApi.Configurations;
using LinguacApi.Services.Email.EmailGenerator;
using LinguacApi.Services.Email.Templates.ConfirmEmail;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email.Confirmation
{
	public class EmailConfirmer(IOptions<EmailConfiguration> emailConfiguration, IEmailGenerator emailGenerator)
	{
		private readonly EmailConfiguration emailConfiguration = emailConfiguration.Value;

		private readonly SendGridClient emailClient = new(emailConfiguration.Value.ApiKey);

		public async Task<DateTime> SendConfirmationEmail(string toAddress, Guid confirmationCode)
		{
			var confirmationLink = emailConfiguration.ConfirmationUrl.AppendPathSegment(confirmationCode);

			var confirmModel = new ConfirmEmailModel(confirmationLink);
			var recipient = new EmailAddress(toAddress);

			var message = await emailGenerator.GenerateEmail(confirmModel, recipient);

			await emailClient.SendEmailAsync(message);

			return DateTime.UtcNow.Add(emailConfiguration.ExpirationTime);
		}
	}
}
