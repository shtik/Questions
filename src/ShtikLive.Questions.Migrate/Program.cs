using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShtikLive.Questions.Data;

namespace ShtikLive.Questions.Migrate
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args.Length == 1
                ? args[0]
                : DesignTimeQuestionContextFactory.LocalPostgres;

            var options = new DbContextOptionsBuilder<QuestionContext>()
                .UseNpgsql(connectionString, b => b.MigrationsAssembly(DesignTimeQuestionContextFactory.MigrationAssemblyName))
                .Options;

            var context = new QuestionContext(options);

            var loggerFactory = new LoggerFactory().AddConsole();

            var migrationHelper = new MigrationHelper(loggerFactory);

            Console.WriteLine("Trying migration...");
            migrationHelper.TryMigrate(context).GetAwaiter().GetResult();
            Console.WriteLine("Done.");
        }
    }
}
