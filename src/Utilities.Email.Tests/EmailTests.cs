using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Configuration;

namespace Utilities.Email.Tests
{
	public class EmailTests
	{
		private readonly IConfiguration _baseConfiguration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string>()
			{
				{"Smtp:SmtpServer", "webmail.bob.com"},
				{"Smtp:Port", "20"},
				{"Smtp:Username", "John"},
				{"Smtp:Password", "1234Password"},
				{"Smtp:EmailFromAddress", "noreply@bob.com"},
				{"Smtp:Environment", "Production"}
			})
			.Build();

		private const string Subject = "Email Test";
		private const string Body = "This is a test.";
		private readonly List<string> _recipients = ["john@bob.com"];
		private readonly List<string> _recipientsCc = ["bob@bob.com"];
		private Attachment Attachment
		{
			get
			{
				using (MemoryStream stream = new())
				{
					using (StreamWriter writer = new(stream))
					{
						writer.Write("Hello its my sample file");
						writer.Flush();
						stream.Position = 0;

						ContentType ct = new(MediaTypeNames.Text.Plain);
						Attachment attachment = new(stream, ct);
						attachment.ContentDisposition.FileName = "myFile.txt";

						return attachment;
					}
				}
			}
		}

		#region Configuration Tests
		[Fact]
		public void SendEmail_ReturnsSystemExceptionSmtpServerIsMissing_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:SmtpServer"] = "";
			
			//Act
			Email email = new(configuration);
			Exception ex = Assert.Throws<Exception>(() => email.SendEmail(Subject, Body, _recipients, _recipientsCc));

			//Assert
			Assert.Contains("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).", ex.Message);
		}


		[Fact]
		public void SendEmail_ReturnsSystemExceptionUsernameIsMissing_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Username"] = "";

			//Act
			Email email = new(configuration);
			Exception ex = Assert.Throws<Exception>(() => email.SendEmail(Subject, Body, _recipients, _recipientsCc));

			//Assert
			Assert.Contains("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).", ex.Message);
		}

		[Fact]
		public void SendEmail_ReturnsSystemExceptionPasswordIsMissing_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Password"] = "";

			//Act
			Email email = new(configuration);
			Exception ex = Assert.Throws<Exception>(() => email.SendEmail(Subject, Body, _recipients, _recipientsCc));

			//Assert
			Assert.Contains("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).", ex.Message);
		}

		[Fact]
		public void SendEmail_ReturnsSystemExceptionFromAddressIsMissing_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:EmailFromAddress"] = "";

			//Act
			Email email = new(configuration);
			Exception ex = Assert.Throws<Exception>(() => email.SendEmail(Subject, Body, _recipients, _recipientsCc));

			//Assert
			Assert.Contains("Missing one or more parameters (Smtp Server, User Name, Password or From Email Address).", ex.Message);
		}
		#endregion

		#region Property Tests
		[Fact]
		public void BuildMessage_ReturnsEmailClass_True()
		{
			//Act
			Email email = new(_baseConfiguration);

			//Assert
			Assert.True(email.SmtpServer == "webmail.bob.com");
			Assert.True(email.Port == 20);
			Assert.True(email.Username == "John");
			Assert.True(email.Password == "1234Password");
			Assert.True(email.FromEmail == "noreply@bob.com");
			Assert.True(email.ServerEnvironment == Email.Environment.Production);
		}
		#endregion

		#region MailMessage Tests
		[Fact]
		public void BuildMessage_ReturnsValidMailMessage_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Environment"] = "LocalDev";
			List<Attachment> attachments = [Attachment];

			//Act
			Email email = new(configuration);
			MailMessage message = email.BuildMessage(Subject, Body, _recipients, _recipientsCc, attachments);

			//Assert
			Assert.True(message.Subject == "Email Test on LocalDev");
			Assert.True(message.Body == Body);
			Assert.True(message.To.Any(a => a.Address == "john@bob.com"));
			Assert.True(message.CC.Any(a => a.Address == "bob@bob.com"));
			Assert.True(message.Attachments.Count == 1);
			Assert.True(message.Attachments.First().ContentDisposition.FileName == "myFile.txt");
		}

		[Fact]
		public void BuildMessage_ReturnsSubjectOnDevelopment_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Environment"] = "Development";

			//Act
			Email email = new(configuration);
			MailMessage message = email.BuildMessage(Subject, Body, _recipients, _recipientsCc, new List<Attachment>());

			//Assert
			Assert.True(message.Subject == "Email Test on Development");
		}

		[Fact]
		public void BuildMessage_ReturnsSubjectOnTest_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Environment"] = "Test";

			//Act
			Email email = new(configuration);
			MailMessage message = email.BuildMessage(Subject, Body, _recipients, _recipientsCc, new List<Attachment>());

			//Assert
			Assert.True(message.Subject == "Email Test on Test");
		}


		[Fact]
		public void BuildMessage_ReturnsSubjectOnProduction_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Environment"] = "Production";

			//Act
			Email email = new(configuration);
			MailMessage message = email.BuildMessage(Subject, Body, _recipients, _recipientsCc, new List<Attachment>());

			//Assert
			Assert.True(message.Subject == Subject);
		}

		[Fact]
		public void BuildMessage_ReturnsSubjectOnLocalDevIfEnumNotFound_True()
		{
			//Arrange
			IConfiguration configuration = _baseConfiguration;
			configuration["Smtp:Environment"] = "Staging";

			//Act
			Email email = new(configuration);
			MailMessage message = email.BuildMessage(Subject, Body, _recipients, _recipientsCc, new List<Attachment>());

			//Assert
			Assert.True(message.Subject == "Email Test on LocalDev");
		}
		#endregion

		#region GetAddress Tests
		[Fact]
		public void BuildMessage_ReturnsValidListOfAddress_True()
		{
			//Act
			Email email = new(_baseConfiguration);

			//Assert
			List<string> recipients = email.GetAddresses("bob@bob.com;john@bob.com;", ";");

			Assert.True(recipients.Count == 2);
			Assert.Contains("bob@bob.com", recipients);
			Assert.Contains("john@bob.com", recipients);
		}

		[Fact]
		public void BuildMessage_ReturnsValidAddressesWithoutReturningEmptyAddress_True()
		{
			//Act
			Email email = new(_baseConfiguration);

			//Added whitespace to the end of the string to verify a non-valid recipient is not returned and the whitespace is trimmed.
			List<string> recipients = email.GetAddresses("bob@bob.com;john@bob.com; ", ";");

			//Assert
			Assert.True(recipients.Count == 2);
			Assert.Contains("bob@bob.com", recipients);
			Assert.Contains("john@bob.com", recipients);
		}

		[Fact]
		public void BuildMessage_ReturnsValidAddressesWithMissingFinalDelimiter_True()
		{
			//Act
			Email email = new(_baseConfiguration);

			//Left off semicolon at the end of the string to verify two recipients are still returned.
			List<string> recipients = email.GetAddresses("bob@bob.com;john@bob.com", ";");

			//Assert
			Assert.True(recipients.Count == 2);
			Assert.Contains("bob@bob.com", recipients);
			Assert.Contains("john@bob.com", recipients);
		}
		#endregion
	}
}