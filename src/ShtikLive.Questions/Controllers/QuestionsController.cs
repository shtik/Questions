using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public QuestionsController(QuestionContext context, ILogger<QuestionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{presenter}/{slug}")]
        public async Task<IActionResult> GetForShow(string presenter, string slug, CancellationToken ct)
        {
            var showIdentifier = $"{presenter}/{slug}";
            List<Question> questions;
            try
            {
                questions = await _context.Questions
                    .Where(q => q.Show == showIdentifier)
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

        [HttpGet("{presenter}/{slug}/{slide:int}")]
        public async Task<IActionResult> GetForSlide(string presenter, string slug, int slide, CancellationToken ct)
        {
            var showIdentifier = $"{presenter}/{slug}";
            List<Question> questions;
            try
            {
                questions = await _context.Questions
                    .Where(q => q.Show == showIdentifier && q.Slide == slide)
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
            var identifier = $"{presenter}/{slug}";

            var question = new Question
            {
                Uuid = Guid.NewGuid().ToString(),
                Show = identifier,
                Slide = slide,
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

            return CreatedAtAction("Get", new {uuid = question.Uuid}, question);
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
            return Ok(answers.Select(a => AnswerDto.FromAnswer(uuid, a)));
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