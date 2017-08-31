using System;
using System.Collections.Generic;
using System.Linq;
using ShtikLive.Questions.Data;

namespace ShtikLive.Questions.Models
{
    public class QuestionDto
    {
        public string Id { get; set; }
        public string Show { get; set; }
        public int Slide { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Time { get; set; }
        public List<AnswerDto> Answers { get; set; }

        public static QuestionDto FromQuestion(Question question)
        {
            return new QuestionDto
            {
                Id = question.Uuid,
                Show = question.Show,
                Slide = question.Slide,
                User = question.User,
                Text = question.Text,
                Time = question.Time,
                Answers = question.Answers?.Select(answer => AnswerDto.FromAnswer(question.Uuid, answer)).ToList()
            };
        }
    }
}