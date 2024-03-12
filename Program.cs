using LinguacApi.Configurations;
using LinguacApi.Data.Database;
using LinguacApi.Data.Models;
using LinguacApi.Services;
using LinguacApi.Services.CookieJwtAuthenticationHandler;
using LinguacApi.Services.JwtHandler;
using LinguacApi.Services.StoryGenerator;
using LinguacApi.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("PromptConfiguration.json");

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
    .Configure<OpenAiConfiguration>(builder.Configuration.GetSection("OpenAiConfiguration"))
    .Configure<PromptConfiguration>(builder.Configuration.GetSection("PromptConfiguration"))
    .Configure<JwtConfiguration>(builder.Configuration.GetSection("JwtConfiguration"))
    .AddTransient<IStoryGenerator, StoryGenerator>()
    .AddScoped<IJwtHandler, JwtHandler>()
    .AddScoped<PasswordHasher<User>>()
    .AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    })
    .AddDbContext<LinguacDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException($"'Database' connection string could not be found in the configuration."));
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
    options.OperationFilter<ExcludeParameterWithAttributeFilter<ModelBinderAttribute>>();
});

var app = builder.Build();

app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<LinguacDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
