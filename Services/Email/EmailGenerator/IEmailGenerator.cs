using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email.EmailGenerator
{
	public interface IEmailGenerator
	{
		Task<SendGridMessage> GenerateEmail<TModel>(TModel model, EmailAddress toAddress) where TModel : IEmailModel;
	}
}