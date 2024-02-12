using LinguacApi.Services.Database;
using LinguacApi.Services.StoryGenerator;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSingleton(builder.Configuration.GetSection("OpenAiConfiguration").Get<OpenAiConfiguration>()
        ?? throw new InvalidOperationException($"'OpenAiConfiguration' could not be found in the configuration or was not formatted properly."))
    .AddTransient<IStoryGenerator, StoryGenerator>()
    .AddDbContext<LinguacDbContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
    })
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
