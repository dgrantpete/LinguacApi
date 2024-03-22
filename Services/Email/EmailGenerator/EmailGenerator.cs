
using RazorLight;
using SendGrid.Helpers.Mail;

namespace LinguacApi.Services.Email.EmailGenerator
{
	public class EmailGenerator(IRazorLightEngine razorEngine) : IEmailGenerator
	{
		public async Task<SendGridMessage> GenerateEmail<TModel>(TModel model, EmailAddress toAddress) where TModel : IEmailModel
		{
			string templateName = typeof(TModel).Name.Replace("Model", "");

			string plainTextContent = await razorEngine.CompileRenderAsync($"Emails/{templateName}/Text", model)
				?? throw new InvalidOperationException($"Error generating text content for {templateName}.");

			var htmlContent = await razorEngine.CompileRenderAsync($"Emails/{templateName}/Html", model)
				?? throw new InvalidOperationException($"Error generating HTML content for {templateName}.");

			var message = MailHelper.CreateSingleEmail(model.Sender, toAddress, model.Subject, plainTextContent, htmlContent);

			message.ReplyTo = model.ReplyTo;

			return message;
		}
	}
}
