using Moq;
using Utilities.Email.MediatR;

namespace Utilities.Email.Tests;

public class MediaRTests
{
	[Fact]
	public void SendEmailHandler_CallsSendEmail()
	{
		//Arrange
		Mock<IEmail> mock = new();
		SendEmailCommand request = new("subject", "body", "recipients", "recipientsCc", "delimiter");
		SendEmailCommandHandler handler = new(mock.Object);

		//Act
		handler.Handle(request, CancellationToken.None);

		//Assert
		mock.Verify(e => e.SendEmail(It.Is<string>(s => s == "subject"),
			It.Is<string>(s => s == "body"), 
			It.Is<string>(s => s == "recipients"), 
			It.Is<string>(s => s == "recipientsCc"), 
			It.Is<string>(s => s == "delimiter")), Times.Once);
		mock.VerifyNoOtherCalls();
	}
}