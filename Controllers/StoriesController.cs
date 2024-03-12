using LinguacApi.Data;
using LinguacApi.Data.Database;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.StoryGenerator;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoriesController(LinguacDbContext dbContext, IStoryGenerator storyGenerator) : ControllerBase
    {
        [HttpGet("{id}")]
        async public Task<ActionResult<StoryDto>> Get(Guid id)
        {
            Story? story = await dbContext.Stories.FindAsync(id);

            return story is null ? NotFound() : Ok(story.ToDto());
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        async public Task<ActionResult> Delete(Guid id)
        {
            Story? story = await dbContext.Stories.FindAsync(id);

            if (story is null)
            {
                return NotFound();
            }

            dbContext.Stories.Remove(story);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        async public Task<ActionResult<IEnumerable<StoryDto>>> GetMultiple(Language? language, CefrLevel? level)
        {
            IQueryable<Story> query = dbContext.Stories;

            if (language.HasValue)
            {
                query = query.Where(story => story.Language == language.Value);
            }

            if (level.HasValue)
            {
                query = query.Where(story => story.Level == level.Value);
            }

            var stories = await query.ToListAsync();

            return Ok(stories.Select(story => story.ToDto()));
        }

        [HttpPost]
        async public Task<ActionResult<StoryDto>> Create(CreateStoryDto createStoryOptions)
        {
            StoryResponse storyResponse = await storyGenerator.GenerateStoryContent(createStoryOptions.Language, createStoryOptions.Level, createStoryOptions.Prompt);

            Story story = new(storyResponse.Title, storyResponse.Content, createStoryOptions.Language, createStoryOptions.Level);

            await dbContext.Stories.AddAsync(story);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { story.Id }, story.ToDto());
        }
    }
}