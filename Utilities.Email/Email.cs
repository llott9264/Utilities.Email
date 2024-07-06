using System.Net;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("Utilities.Email.Tests")]
namespace Utilities.Email;

public class Email : IEmail
{
	public enum Environment
	{
		LocalDev,
		Development,
		Test,
		Production
	}

	private readonly string _smtpServer;
	private readonly int _port;
	private readonly string _username;
	private readonly string _password;
	private readonly string _fromEmail;
	private readonly Environment _environment;

	public Email(IConfiguration configuration)
	{
		_smtpServer = configuration.GetValue<string>("Smtp:SmtpServer") ?? string.Empty;
		_port = configuration.GetValue<int>("Smtp:Port") != 0
			? configuration.GetValue<int>("Smtp:Port")
			: 25;
		_username = configuration.GetValue<string>("Smtp:Username") ?? string.Empty;
		_password = configuration.GetValue<string>("Smtp:Password") ?? string.Empty;
		_fromEmail = configuration.GetValue<string>("Smtp:EmailFromAddress") ?? string.Empty;
		_environment = Enum.TryParse(configuration.GetValue<string>("Smtp:Environment") ?? string.Empty, true, out Environment environment)
			? environment
			: Environment.LocalDev;
	}

	//Smtp Settings
	public string SmtpServer => _smtpServer;
	public int Port => _port;
	public string Username => _username;
	public string Password => _password;
	public string FromEmail => _fromEmail;
	public Environment ServerEnvironment => _environment;


	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter)
	{
		SendEmail(subject, body, recipients, recipientsCc, delimiter, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter, List<Attachment> attachments)
	{
		SendEmail(subject, body,
			recipients.Split(delimiter).Where(recipient => recipient.Length > 0).ToList(),
			recipientsCc.Split(delimiter).Where(recipient => recipient.Length > 0).ToList(),
			attachments);
	}

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc)
	{
		SendEmail(subject, body, recipients, recipientsCc, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		MailMessage message = BuildMessage(subject, body, recipients, recipientsCc, attachments);
		Send(message);
	}

	private void Send(MailMessage message)
	{
		if (_smtpServer == string.Empty || _username == string.Empty ||
		    _password == string.Empty || _fromEmail == string.Empty)
		{
			throw new Exception("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).");
		}

		using (SmtpClient client = new(_smtpServer, _port))
		{
			client.Credentials = new NetworkCredential(_username, _password);
			client.EnableSsl = false;
			client.Send(message);
		}
	}

	internal MailMessage BuildMessage(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		MailMessage message = new()
		{
			From = new MailAddress(_fromEmail),
			Body = body,
			IsBodyHtml = true,
			Subject = _environment switch
			{
				Environment.LocalDev
					or Environment.Development
					or Environment.Test => subject + " on " + _environment,
				_ => subject
			}
		};

		attachments.ForEach(a => message.Attachments.Add(a));
		recipients.ForEach(r => message.To.Add(r));
		recipientsCc.ForEach(r => message.CC.Add(r));

		return message;
	}
}