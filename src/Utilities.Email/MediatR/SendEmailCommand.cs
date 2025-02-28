using MediatR;

namespace Utilities.Email.MediatR;

public class SendEmailCommand(string subject, string body, string recipients, string recipientsCc, string delimiter) : IRequest
{
	public string Subject { get; } = subject;
	public string Body { get; } = body;
	public string Recipients { get; } = recipients;
	public string RecipientsCc { get; } = recipientsCc;
	public string Delimiter { get; } = delimiter;
}