using LinguacApi.Services.Database;
using LinguacApi.Services.StoryGenerator;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("PromptConfiguration.json");

builder.Services
    .AddRouting(options => options.LowercaseUrls = true)
    .Configure<OpenAiConfiguration>(builder.Configuration.GetSection("OpenAiConfiguration"))
    .Configure<PromptConfiguration>(builder.Configuration.GetSection("PromptConfiguration"))
    .AddTransient<IStoryGenerator, StoryGenerator>()
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
    .AddControllers()
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
builder.Services.AddSwaggerGen();

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
