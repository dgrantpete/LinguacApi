using LinguacApi.Data;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.Database;
using LinguacApi.Services.StoryGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("Story/{storyId}/[controller]")]
    public class QuestionController(LinguacDbContext dbContext, IStoryGenerator storyGenerator) : ControllerBase
    {
        [HttpPost]
        async public Task<ActionResult<IEnumerable<QuestionDto>>> GetOrCreateMultiple(Guid storyId)
        {
            Story? story = await dbContext.Stories.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == storyId);

            if (story is null)
            {
                return NotFound();
            }

            if (story.Questions.Count <= 0)
            {
                IEnumerable<string> questionTexts = await storyGenerator.GenerateStoryQuestions(story.Content, story.Language, story.Level);
                await dbContext.Questions.AddRangeAsync(questionTexts
                    .Select(questionText => new Question(Guid.NewGuid(), questionText) { Story = story }));

                await dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMultiple), new { storyId }, story.Questions.Select(question => question.ToDto()));
            }

            return Ok(story.Questions.Select(question => question.ToDto()));
        }

        [HttpGet]
        async public Task<ActionResult<IEnumerable<QuestionDto>>> GetMultiple(Guid storyId)
        {
            Story? story = await dbContext.Stories.Include(s => s.Questions).FirstOrDefaultAsync(s => s.Id == storyId);

            return story is null || story.Questions.Count <= 0
                ? NotFound()
                : Ok(story.Questions.Select(question => question.ToDto()));
        }
    }
}