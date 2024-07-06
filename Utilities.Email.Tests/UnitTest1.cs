using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Utilities.Email.Tests
{
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{ 
			//Arrange
			IConfiguration configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string>()
				{
					{"Smtp:SmtpServer", ""},
					{"Smtp:Port", "25"},
					{"Smtp:Username", "John"},
					{"Smtp:Password", "1234Password"},
					{"Smtp:EmailFromAddress", "noreply@bob.com"},
					{"Smtp:Environment", "Test"}
				})
				.Build();

			string subject = "Email Test";
			string body = "This is a test.";
			List<string> recipients = ["john@bob.com"];
			List<string> recipientsCc = ["bob@bob.com"];

			//Act
			Email email = new(configuration);
			Exception ex = Assert.Throws<Exception>(() => email.SendEmail(subject, body, recipients, recipientsCc));

			//Assert
			Assert.Contains("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).", ex.Message);
		}
	}
}