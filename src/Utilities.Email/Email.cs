using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Utilities.Email.Tests")]
namespace Utilities.Email;

public class Email(IConfiguration configuration) : IEmail
{
	public enum Environment
	{
		LocalDev,
		Development,
		Test,
		Production
	}

	public string SmtpServer => configuration.GetValue<string>("Smtp:SmtpServer") ?? string.Empty;
	public int Port => configuration.GetValue<int>("Smtp:Port") != 0
		? configuration.GetValue<int>("Smtp:Port")
		: 25;
	public string Username => configuration.GetValue<string>("Smtp:Username") ?? string.Empty;
	public string Password => configuration.GetValue<string>("Smtp:Password") ?? string.Empty;
	public string FromEmail => configuration.GetValue<string>("Smtp:EmailFromAddress") ?? string.Empty;
	public Environment ServerEnvironment => Enum.TryParse(configuration.GetValue<string>("Smtp:Environment") ?? string.Empty, true, out Environment environment)
		? environment
		: Environment.LocalDev;


	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter)
	{
		SendEmail(subject, body, recipients, recipientsCc, delimiter, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter, List<Attachment> attachments)
	{
		SendEmail(subject, body, GetAddresses(recipients, delimiter), GetAddresses(recipientsCc, delimiter), attachments);
	}

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc)
	{
		SendEmail(subject, body, recipients, recipientsCc, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		Send(subject, body, recipients, recipientsCc, attachments);
	}

	private void Send(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		if (SmtpServer == string.Empty || Username == string.Empty ||
		    Password == string.Empty || FromEmail == string.Empty)
		{
			throw new Exception("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).");
		}

		MailMessage message = BuildMessage(subject, body, recipients, recipientsCc, attachments);

		using (SmtpClient client = new(SmtpServer, Port))
		{
			client.Credentials = new NetworkCredential(Username, Password);
			client.EnableSsl = false;
			client.Send(message);
		}
	}

	internal MailMessage BuildMessage(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		MailMessage message = new()
		{
			From = new MailAddress(FromEmail),
			Body = body,
			IsBodyHtml = true,
			Subject = ServerEnvironment switch
			{
				Environment.LocalDev
					or Environment.Development
					or Environment.Test => subject + " on " + ServerEnvironment,
				_ => subject
			}
		};

		attachments.ForEach(a => message.Attachments.Add(a));
		recipients.ForEach(r => message.To.Add(r));
		recipientsCc.ForEach(r => message.CC.Add(r));

		return message;
	}

	internal List<string> GetAddresses(string recipients, string delimiter)
	{
		return recipients.Split(delimiter).Where(recipient => recipient.Trim().Length > 0).ToList();
	}
}