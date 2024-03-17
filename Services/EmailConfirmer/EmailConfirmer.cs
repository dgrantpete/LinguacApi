using Flurl;
using LinguacApi.Configurations;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.EmailConfirmer
{
    public class EmailConfirmer
    {
        private readonly SendGridClient _emailClient;

        private readonly EmailAddress _fromAddress;

        private readonly EmailAddress _replyToAddress;

        private readonly string _confirmationUrl;

        private readonly string _subject;

        private readonly string _htmlContent;

        private readonly string _plainTextContent;

        private readonly TimeSpan _expirationTime;

        public EmailConfirmer(IOptions<EmailConfirmationConfiguration> emailConfiguration)
        {
            var emailConfigurationValue = emailConfiguration.Value;
            _emailClient = new SendGridClient(emailConfigurationValue.ApiKey);
            _fromAddress = new EmailAddress(emailConfigurationValue.FromEmail, emailConfigurationValue.FromName);
            _replyToAddress = new EmailAddress(emailConfigurationValue.ReplyToEmail, emailConfigurationValue.ReplyToName);
            _confirmationUrl = emailConfigurationValue.ConfirmationUrl;
            _subject = emailConfigurationValue.Subject;
            _htmlContent = emailConfigurationValue.HtmlContent;
            _plainTextContent = emailConfigurationValue.PlainTextContent;
            _expirationTime = emailConfigurationValue.ExpirationTime;
        }

        public async Task<DateTime> SendConfirmationEmail(string email, Guid confirmationCode)
        {
            var confirmationLink = _confirmationUrl.AppendPathSegment(confirmationCode);
            var toAddress = new EmailAddress(email);
            var plainTextContent = string.Format(_plainTextContent, confirmationLink);
            var htmlContent = string.Format(_htmlContent, confirmationLink);
            var msg = MailHelper.CreateSingleEmail(_fromAddress, toAddress, _subject, plainTextContent, htmlContent);
            msg.ReplyTo = _replyToAddress;
            await _emailClient.SendEmailAsync(msg);

            return DateTime.UtcNow.Add(_expirationTime);
        }
    }
}
