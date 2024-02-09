using LinguacApi.Data;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.Database;
using LinguacApi.Services.StoryGenerator;
using Microsoft.AspNetCore.Mvc;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController(LinguacDbContext dbContext, IStoryGenerator storyGenerator) : ControllerBase
    {
        [HttpGet("{id}")]
        async public Task<ActionResult<StoryDto>> Get(Guid id)
        {
            Story? story = await dbContext.Stories.FindAsync(id);

            return story is null ? NotFound() : Ok(story.ToDto());
        }

        [HttpPost]
        async public Task<ActionResult<StoryDto>> Create(CreateStoryDto createStoryOptions)
        {
            StoryResponse storyResponse = await storyGenerator.GenerateStoryContent(createStoryOptions.Language, createStoryOptions.Level);

            Story story = new(Guid.NewGuid(), storyResponse.Title, storyResponse.Content, createStoryOptions.Language, createStoryOptions.Level);

            await dbContext.Stories.AddAsync(story);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { story.Id }, story.ToDto());
        }
    }
}