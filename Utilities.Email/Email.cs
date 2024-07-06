using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

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

	private string _subject = string.Empty;
	private string _body = string.Empty;
	private List<string> _recipients = new();
	private List<string> _recipientsCc = new();
	private List<Attachment> _attachments = new();
	
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
	

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc)
	{
		SendEmail(subject, body, recipients, recipientsCc, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments)
	{
		_subject = subject;
		_body = body;
		_recipients = recipients;
		_recipientsCc = recipientsCc;
		_attachments = attachments;

		Send();
	}

	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter)
	{
		SendEmail(subject, body, recipients, recipientsCc, delimiter, new List<Attachment>());
	}

	public void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter, List<Attachment> attachments)
	{
		_subject = subject;
		_body = body;
		_attachments = attachments;

		foreach (string recipient in recipients.Split(delimiter))
		{
			if (recipient.Length > 0)
			{
				_recipients.Add(recipient);
			}
		}

		foreach (string recipient in recipientsCc.Split(delimiter))
		{
			if (recipient.Length > 0)
			{
				_recipientsCc.Add(recipient);
			}
		}
		
		Send();
	}

	private void Send()
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
			client.Send(BuildMessage());
		}
	}

	private MailMessage BuildMessage()
	{
		MailMessage message = new()
		{
			From = new MailAddress(_fromEmail),
			Body = _body,
			IsBodyHtml = true,
			Subject = _environment switch
			{
				Environment.LocalDev
					or Environment.Development
					or Environment.Test => _subject + " on " + _environment,
				_ => _subject
			}
		};

		_attachments.ForEach(a => message.Attachments.Add(a));
		_recipients.ForEach(r => message.To.Add(r));
		_recipientsCc.ForEach(r => message.CC.Add(r));

		return message;
	}
}