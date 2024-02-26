using LinguacApi.Data;
using LinguacApi.Data.Dtos;
using LinguacApi.Data.Models;
using LinguacApi.Services.Database;
using LinguacApi.Services.StoryGenerator;
using LinguacApi.Services.StoryGenerator.OpenAiModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinguacApi.Controllers
{
    [ApiController]
    [Route("Story/{storyId}/[controller]")]
    public class QuestionsController(LinguacDbContext dbContext, IStoryGenerator storyGenerator) : ControllerBase
    {
        [HttpPost]
        async public Task<ActionResult<IEnumerable<QuestionDto>>> GetOrCreateMultiple(Guid storyId)
        {
            Story? story = await dbContext.Stories
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == storyId);

            if (story is null)
            {
                return NotFound();
            }

            if (story.Questions.Count <= 0)
            {
                IEnumerable<QuestionResponse> questionTexts = await storyGenerator.GenerateStoryQuestions(story.Content, story.Language, story.Level);
                await dbContext.Questions.AddRangeAsync(questionTexts
                    .Select(questionResponse =>
                    {
                        Question question = new(questionResponse.Text) { Story = story };
                        IEnumerable<Answer> answers = questionResponse.WrongAnswers
                            .Select(answerText => new Answer(answerText, false) { Question = question })
                            .Append(new Answer(questionResponse.CorrectAnswer, true) { Question = question });

                        foreach (Answer answer in answers)
                        {
                            question.Answers.Add(answer);
                        }

                        return question;
                    }));

                await dbContext.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMultiple), new { storyId }, story.Questions.Select(question => question.ToDto()));
            }

            return Ok(story.Questions.Select(question => question.ToDto()));
        }

        [HttpGet]
        async public Task<ActionResult<IEnumerable<QuestionDto>>> GetMultiple(Guid storyId)
        {
            Story? story = await dbContext.Stories
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(s => s.Id == storyId);

            return story is null || story.Questions.Count <= 0
                ? NotFound()
                : Ok(story.Questions.Select(question => question.ToDto()));
        }
    }
}