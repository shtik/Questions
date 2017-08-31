using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShtikLive.Questions.Data
{
    public class Question
    {
        public int Id { get; set; }

        [MaxLength(40)]
        public string Uuid { get; set; }

        [MaxLength(256)]
        public string Show { get; set; }

        public int Slide { get; set; }

        [MaxLength(16)]
        public string User { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Time { get; set; }

        public List<Answer> Answers { get; set; }
    }
}