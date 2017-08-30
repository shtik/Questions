using System;

namespace ShtikLive.Questions.Models
{
    public class QuestionMessage
    {
        public string MessageType { get; set; } = "question";
        public string Id { get; set; }
        public string Presenter { get; set; }
        public string Slug { get; set; }
        public int Slide { get; set; }
        public string User { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Text { get; set; }
    }
}