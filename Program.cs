using LinguacApi.Data;
using LinguacApi.Data.Binders;
using LinguacApi.Data.Models;
using LinguacApi.Extensions;
using LinguacApi.Services.StoryGenerator;
using LinguacApi.Services.StoryGenerator.PromptGeneration;
using LinguacApi.Swagger;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
	.AddDynamicConfigurations("LinguacApi.Configurations")
	.Configure<JsonSerializerOptions>(SerializerOptions.OpenAiSerializer, options =>
	{
		options.PropertyNameCaseInsensitive = true;
		options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
		options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
	})
	.SetupAuthentication(configuration)
	.SetupDataPersistence(configuration)
	.SetupEmail()
	.AddRouting()
	.AddScoped<IStoryGenerator, StoryGenerator>()
	.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
	.AddSingleton<IPromptGenerator, PromptGenerator>()
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
