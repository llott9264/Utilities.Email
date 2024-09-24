using System.Net.Mail;

namespace Utilities.Email;

public interface IEmail
{
	public string SmtpServer { get; }
	public int Port { get; }
	public string Username { get; }
	public string Password { get; }
	public string FromEmail { get; }
	public Email.Environment ServerEnvironment { get; }

	void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc);
	void SendEmail(string subject, string body, List<string> recipients, List<string> recipientsCc, List<Attachment> attachments);
	void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter);
	void SendEmail(string subject, string body, string recipients, string recipientsCc, string delimiter, List<Attachment> attachments);
}