using Flurl;
using LinguacApi.Configurations;
using LinguacApi.Services.Email.EmailGenerator;
using LinguacApi.Services.Email.Templates.ConfirmEmail;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email.Confirmation
{
	public class EmailConfirmer
	{
		private readonly SendGridClient _emailClient;

		private readonly string _confirmationUrl;

		private readonly TimeSpan _expirationTime;

		private readonly IEmailGenerator _emailGenerator;

		public EmailConfirmer(IOptions<EmailConfiguration> emailConfiguration, IEmailGenerator emailGenerator)
		{
			_emailGenerator = emailGenerator;
			var emailConfigurationValue = emailConfiguration.Value;
			_emailClient = new SendGridClient(emailConfigurationValue.ApiKey);
			_confirmationUrl = emailConfigurationValue.ConfirmationUrl;
			_expirationTime = emailConfigurationValue.ExpirationTime;
		}

		public async Task<DateTime> SendConfirmationEmail(string toAddress, Guid confirmationCode)
		{
			var confirmationLink = _confirmationUrl.AppendPathSegment(confirmationCode);

			var message = await _emailGenerator.GenerateEmail(new ConfirmEmailModel(confirmationLink), new EmailAddress(toAddress));

			await _emailClient.SendEmailAsync(message);

			return DateTime.UtcNow.Add(_expirationTime);
		}
	}
}
