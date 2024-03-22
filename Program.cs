using LinguacApi.Data.Binders;
using LinguacApi.Data.Database;
using LinguacApi.Data.Models;
using LinguacApi.Extensions;
using LinguacApi.Services.Authentication.CookieJwtAuthenticationHandler;
using LinguacApi.Services.Authentication.JwtHandler;
using LinguacApi.Services.Email.Confirmation;
using LinguacApi.Services.Email.EmailGenerator;
using LinguacApi.Services.StoryGenerator;
using LinguacApi.Services.StoryGenerator.PromptGeneration;
using LinguacApi.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using RazorLight.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("promptConfiguration.json");

builder.Services
	.AddAuthentication("CookieJwt")
	.AddScheme<CookieJwtAuthenticationOptions, CookieJwtAuthenticationHandler>("CookieJwt", options =>
	{
		options.ValidateAccessTokenDelegate = (jwtHandler, jwt) => jwtHandler.ValidateAccessToken(jwt);
		options.GetCookieNameDelegate = jwtConfiguration => jwtConfiguration.AccessCookieName;
	})
	.AddScheme<CookieJwtAuthenticationOptions, CookieJwtAuthenticationHandler>("RefreshCookieJwt", options =>
	{
		options.ValidateAccessTokenDelegate = (jwtHandler, jwt) => jwtHandler.ValidateRefreshToken(jwt);
		options.GetCookieNameDelegate = jwtConfiguration => jwtConfiguration.RefreshCookieName;
	});

builder.Services.AddAuthorizationBuilder()
	.SetFallbackPolicy(new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.RequireRole("user")
			.Build());

builder.Services
	.AddRouting()
	.AddScoped<IStoryGenerator, StoryGenerator>()
	.AddSingleton<IJwtHandler, JwtHandler>()
	.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
	.AddSingleton<EmailConfirmer>()
	.AddSingleton<IEmailGenerator, EmailGenerator>()
	.AddSingleton<IPromptGenerator, PromptGenerator>()
	.AddDynamicConfigurations("LinguacApi.Configurations")
	.Configure<JsonSerializerOptions>("OpenAiSerializer", options =>
	{
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
		options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
	})
	.AddRazorLight(
		() => new RazorLightEngineBuilder()
			.UseFileSystemProject(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates"))
			.UseMemoryCachingProvider()
			.Build())
	.AddCors(options =>
	{
		options.AddPolicy("Development", builder =>
		{
			builder.SetIsOriginAllowed(_ => true)
				.AllowCredentials()
				.AllowAnyHeader()
				.AllowAnyMethod();
		});
	})
	.AddDbContext<LinguacDbContext>(options =>
	{
		options.UseNpgsql(builder.Configuration.GetConnectionString("Database")
			?? throw new InvalidOperationException($"'Database' connection string could not be found in the configuration."));

		options.UseSnakeCaseNamingConvention();
	})
	.AddControllers(options =>
	{
		options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
	})
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	});

builder.Services.AddHsts(options =>
{
	options.Preload = false;
	options.IncludeSubDomains = false;
	// Using shorter max age while proving this configuration. Will update to 1 year if configuration works as expected.
	options.MaxAge = TimeSpan.FromDays(14);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.OperationFilter<ExcludeParameterWithAttributeFilter<AuthenticatedUserAttribute>>();
});

var app = builder.Build();

app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwaggerUI();
	app.UseCors("Development");
}

if (app.Environment.IsProduction())
{
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
