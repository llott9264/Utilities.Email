using Microsoft.Extensions.DependencyInjection;

namespace Utilities.Email;

public static class EmailServiceRegistration
{
	public static IServiceCollection AddEmailServices(this IServiceCollection services)
	{
		services.AddSingleton<IEmail, Email>();
		return services;
	}
}