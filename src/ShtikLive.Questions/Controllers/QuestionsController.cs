using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ShtikLive.Questions.Data;
using ShtikLive.Questions.Models;
using StackExchange.Redis;

namespace ShtikLive.Questions.Controllers
{
    public class QuestionsController : Controller
    {
        private readonly QuestionContext _context;
        private readonly ILogger<QuestionsController> _logger;
        private readonly ConnectionMultiplexer _redis;

        public QuestionsController(QuestionContext context, ILogger<QuestionsController> logger, ConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
        }

        [HttpGet("{presenter}/{slug}/{slide:int}")]
        public async Task<IActionResult> GetForSlide(string presenter, string slug, int slide, CancellationToken ct)
        {
            var identifier = $"{presenter}/{slug}/{slide}";
            List<Question> questions;
            try
            {
                questions = await _context.Questions
                    .Where(q => q.SlideIdentifier == identifier)
                    .OrderBy(q => q.Time)
                    .Include(q => q.Answers)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }
            return Ok(questions.Select(QuestionDto.FromQuestion));
        }

        [HttpGet("{uuid}")]
        public async Task<IActionResult> Get(string uuid, CancellationToken ct)
        {
            Question question;
            try
            {
                question = await _context.Questions
                    .SingleOrDefaultAsync(q => q.Uuid == uuid, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }
            if (question == null) return NotFound();
            return Ok(QuestionDto.FromQuestion(question));
        }

        [HttpPost("{presenter}/{slug}/{slide:int}")]
        public async Task<IActionResult> Ask(string presenter, string slug, int slide,
            [FromBody] QuestionDto dto, CancellationToken ct)
        {
            var identifier = $"{presenter}/{slug}/{slide}";

            var question = new Question
            {
                Uuid = Guid.NewGuid().ToString(),
                SlideIdentifier = identifier,
                Text = dto.Text,
                User = dto.User,
                Time = dto.Time
            };

            try
            {
                _context.Questions.Add(question);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }

            await PublishQuestionMessage(presenter, slug, slide, question).ConfigureAwait(false);

            return CreatedAtAction("Get", new {uuid = question.Uuid}, question);
        }

        private Task PublishQuestionMessage(string presenter, string slug, int slide, Question question)
        {
            var message = new QuestionMessage
            {
                Id = question.Uuid,
                Presenter = presenter,
                Slug = slug,
                Slide = slide,
                User = question.User,
                Time = question.Time,
                Text = question.Text
            };

            var json = JsonConvert.SerializeObject(message);
            return _redis.GetDatabase().PublishAsync("shtik:messaging", json);
        }

        [HttpPost("{uuid}/answers")]
        public async Task<IActionResult> Answers(string uuid, CancellationToken ct)
        {
            List<Answer> answers;
            try
            {
                answers = await _context.Answers
                    .Where(a => a.Question.Uuid == uuid)
                    .OrderBy(a => a.Time)
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }
            return Ok(answers.Select(AnswerDto.FromAnswer));
        }

        [HttpPost("{uuid}/answers")]
        public async Task<IActionResult> Answer(string uuid, [FromBody] AnswerDto dto, CancellationToken ct)
        {
            try
            {
                var question = await _context.Questions
                    .SingleOrDefaultAsync(q => q.Uuid == uuid, ct);
                if (question == null) return NotFound();

                var answer = new Answer
                {
                    QuestionId = question.Id,
                    User = dto.User,
                    Text = dto.Text,
                    Time = dto.Time
                };

                _context.Answers.Add(answer);
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.DatabaseError, ex, ex.Message);
                throw;
            }
            return Accepted();
        }
    }
}