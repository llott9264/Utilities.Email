using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Utilities.Email.MediatR;

namespace Utilities.Email.Tests;

public class EmailServiceRegistrationTests
{
	private readonly IConfiguration _configuration = new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>
		{
			{ "Gpg:KeyFolderPath", "Value1" }
		})
		.Build();

	[Fact]
	public void AddEmailServices_RegistersAllServices_CorrectlyResolvesTypes()
	{
		// Arrange
		ServiceCollection services = new();
		IConfiguration configuration = _configuration;
		services.AddSingleton(configuration);

		// Act
		_ = services.AddEmailServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IMediator? mediator = serviceProvider.GetService<IMediator>();
		IEmail? gpg = serviceProvider.GetService<IEmail>();

		// Assert
		Assert.NotNull(mediator);
		_ = Assert.IsType<Mediator>(mediator);

		Assert.NotNull(gpg);
		_ = Assert.IsType<Email>(gpg);
	}

	[Fact]
	public void AddEmailServices_ReturnsServiceCollection()
	{
		// Arrange
		ServiceCollection services = new();
		IConfiguration configuration = _configuration;
		services.AddSingleton(configuration);

		// Act
		IServiceCollection result = services.AddEmailServices();

		// Assert
		Assert.Same(services, result); // Ensures the method returns the same IServiceCollection
	}

	[Fact]
	public void AddEmailServices_ScopedLifetime_VerifyInstanceWithinScope()
	{
		// Arrange
		ServiceCollection services = new();
		IConfiguration configuration = _configuration;
		services.AddSingleton(configuration);

		// Act
		_ = services.AddEmailServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		using IServiceScope scope = serviceProvider.CreateScope();
		IMediator? service1 = scope.ServiceProvider.GetService<IMediator>();
		IMediator? service2 = scope.ServiceProvider.GetService<IMediator>();
		IEmail? service3 = scope.ServiceProvider.GetService<IEmail>();
		IEmail? service4 = scope.ServiceProvider.GetService<IEmail>();

		Assert.NotSame(service1, service2);
		Assert.Same(service3, service4);
	}

	[Fact]
	public void AddEmailServices_ScopedLifetime_VerifyInstancesAcrossScopes()
	{
		// Arrange
		ServiceCollection services = new();
		IConfiguration configuration = _configuration;
		services.AddSingleton(configuration);

		// Act
		_ = services.AddEmailServices();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		// Assert
		IMediator? service1, service2;
		IEmail? service3, service4;
		using (IServiceScope scope1 = serviceProvider.CreateScope())
		{
			service1 = scope1.ServiceProvider.GetService<IMediator>();
			service3 = scope1.ServiceProvider.GetService<IEmail>();
		}

		using (IServiceScope scope2 = serviceProvider.CreateScope())
		{
			service2 = scope2.ServiceProvider.GetService<IMediator>();
			service4 = scope2.ServiceProvider.GetService<IEmail>();
		}

		Assert.NotSame(service1, service2);
		Assert.Same(service3, service4);
	}

	[Fact]
	public void AddEmailServices_CleanUpDirectoryHandler_VerifyMediatorHandlerExists()
	{
		// Arrange
		ServiceCollection services = new();

		// Act
		_ = services.AddEmailServices();
		List<ServiceDescriptor> serviceDescriptors = services.ToList();

		// Assert
		ServiceDescriptor? handlerDescriptor = serviceDescriptors.FirstOrDefault(sd =>
			sd.ServiceType == typeof(IRequestHandler<SendEmailCommand>));

		Assert.NotNull(handlerDescriptor);
		Assert.Equal(ServiceLifetime.Transient, handlerDescriptor.Lifetime);
	}
}