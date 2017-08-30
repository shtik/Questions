using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ShtikLive.Questions.Data;

namespace ShtikLive.Questions.Migrate
{
    public class DesignTimeQuestionContextFactory : IDesignTimeDbContextFactory<QuestionContext>
    {
        public const string LocalPostgres = "Host=localhost;Database=questions;Username=shtik;Password=secretsquirrel";

        public static readonly string MigrationAssemblyName =
            typeof(DesignTimeQuestionContextFactory).Assembly.GetName().Name;

        public QuestionContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<QuestionContext>()
                .UseNpgsql(LocalPostgres, b => b.MigrationsAssembly(MigrationAssemblyName));
            return new QuestionContext(builder.Options);
        }
    }
}