using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace ShtikLive.Questions.Data
{
    public class QuestionContext : DbContext
    {
        public QuestionContext([NotNull] DbContextOptions options) : base(options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Question>()
                .HasIndex(q => q.Uuid)
                .IsUnique();

            builder.Entity<Question>()
                .HasIndex(q => q.SlideIdentifier)
                .IsUnique(false);
        }
    }
}
