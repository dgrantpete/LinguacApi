using LinguacApi.Configurations;
using LinguacApi.Data.Database;
using LinguacApi.Services.Authentication;
using LinguacApi.Services.Authentication.CookieAuthenticationHandler;
using LinguacApi.Services.Authentication.TokenHandler;
using LinguacApi.Services.Email.Confirmation;
using LinguacApi.Services.Email.EmailGenerator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using RazorLight.Extensions;

namespace LinguacApi.Extensions
{
	public static class ServiceConfigurationExtensions
	{
		public static IServiceCollection AddConfiguration<TOptions>(
			this IServiceCollection services,
			string? configSectionPath = default
		) where TOptions : class
		{
			services.AddOptionsWithValidateOnStart<TOptions>()
				.BindConfiguration(configSectionPath ?? typeof(TOptions).Name)
				.ValidateDataAnnotations();

			return services;
		}

		public static IServiceCollection AddDynamicConfigurations(this IServiceCollection services, string configurationsNamespace)
		{
			var configurationsAssembly = typeof(ServiceConfigurationExtensions).Assembly;

			var configurationTypes = configurationsAssembly.GetTypes()
				.Where(type => type.IsClass && type.Namespace == configurationsNamespace);

			foreach (var configurationType in configurationTypes)
			{
				typeof(ServiceConfigurationExtensions).GetMethod(nameof(AddConfiguration))
					?.MakeGenericMethod(configurationType)
					?.Invoke(null, [services, null]);
			}

			return services;
		}

		public static IServiceCollection SetupAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			AuthTokenConfiguration tokenConfiguration = configuration.GetSection(nameof(AuthTokenConfiguration)).Get<AuthTokenConfiguration>()
				?? throw new InvalidOperationException($"{nameof(AuthTokenConfiguration)} is invalid or missing");

			services.AddAuthentication(AuthenticationSchemes.AccessToken)
				.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(AuthenticationSchemes.AccessToken, options =>
				{
					options.ValidateAccessTokenDelegate = (tokenHandler, token) => tokenHandler.ValidateAccessToken(token);
					options.CookieName = tokenConfiguration.AccessCookieName;
				})
				.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(AuthenticationSchemes.RefreshToken, options =>
				{
					options.ValidateAccessTokenDelegate = (tokenHandler, token) => tokenHandler.ValidateRefreshToken(token);
					options.CookieName = tokenConfiguration.RefreshCookieName;
				});

			services.AddSingleton<ITokenHandler, JwtTokenHandler>();

			services.AddAuthorizationBuilder()
				.SetFallbackPolicy(new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.RequireRole("user")
				.Build());

			return services;
		}

		public static IServiceCollection SetupDataPersistence(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<LinguacDbContext>(options =>
			{
				options.UseNpgsql(configuration.GetConnectionString("Database")
					?? throw new InvalidOperationException($"'Database' connection string could not be found in the configuration."));

				options.UseSnakeCaseNamingConvention();
			});

			return services;
		}

		public static IServiceCollection SetupEmail(this IServiceCollection services)
		{
			services.AddSingleton<EmailConfirmer>()
				.AddSingleton<IEmailGenerator, EmailGenerator>()
				.AddRazorLight(() => new RazorLightEngineBuilder()
					.UseFileSystemProject(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"))
					.UseMemoryCachingProvider()
					.Build());

			return services;
		}
	}
}
