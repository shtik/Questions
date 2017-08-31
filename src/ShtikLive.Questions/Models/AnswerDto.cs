using System;
using ShtikLive.Questions.Data;

namespace ShtikLive.Questions.Models
{
    public class AnswerDto
    {
        public string QuestionId { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Time { get; set; }

        public static AnswerDto FromAnswer(string uuid, Answer answer)
        {
            return new AnswerDto
            {
                QuestionId = uuid,
                User = answer.User,
                Text = answer.Text,
                Time = answer.Time
            };
        }
    }
}