using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email
{
	public interface IEmailModel
	{
		string Name => typeof(IEmailModel).Name.Replace("Model", "");

		string Subject { get; }

		EmailAddress Sender { get; }

		EmailAddress ReplyTo => Sender;
	}
}