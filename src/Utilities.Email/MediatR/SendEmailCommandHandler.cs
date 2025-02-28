using MediatR;

namespace Utilities.Email.MediatR;

public class SendEmailCommandHandler(IEmail email) : IRequestHandler<SendEmailCommand>
{
	public Task Handle(SendEmailCommand request, CancellationToken cancellationToken)
	{
		email.SendEmail(request.Subject, request.Body, request.Recipients, request.RecipientsCc, request.Delimiter);
		return Task.CompletedTask;
	}
}