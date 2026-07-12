using QuizApi.Models;

namespace QuizApi.Data;

/// <summary>
/// Idempotent code seeder. Populates a fresh (empty) database with demo data so
/// both local dev and the ephemeral-SQLite Azure deployment always have content.
/// Safe to call on every startup — it no-ops once quizzes exist.
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        // Already seeded — nothing to do.
        if (db.Quizzes.Any()) return;

        // --- Categories ---
        var general = new Category { Name = "General Knowledge" };
        var science = new Category { Name = "Science" };
        var programming = new Category { Name = "Programming" };
        db.Categories.AddRange(general, science, programming);
        db.SaveChanges();

        // --- Quizzes with questions + options ---
        // Each question carries 4 options; the correct one is flagged locally,
        // then wired into Question.CorrectOptionId after IDs are assigned.
        var quizzes = new[]
        {
            BuildQuiz("World Capitals", "Test your geography basics.", "Easy", general.Id, new[]
            {
                Q("What is the capital of France?", "Paris", "London", "Berlin", "Madrid"),
                Q("What is the capital of Japan?", "Tokyo", "Beijing", "Seoul", "Bangkok"),
                Q("What is the capital of Australia?", "Canberra", "Sydney", "Melbourne", "Perth"),
            }),
            BuildQuiz("Basic Science", "Fundamental science questions.", "Medium", science.Id, new[]
            {
                Q("What planet is known as the Red Planet?", "Mars", "Venus", "Jupiter", "Saturn"),
                Q("What gas do plants absorb from the atmosphere?", "Carbon dioxide", "Oxygen", "Nitrogen", "Hydrogen"),
                Q("What is the chemical symbol for water?", "H2O", "CO2", "O2", "NaCl"),
            }),
            BuildQuiz("Programming Fundamentals", "Core software concepts.", "Hard", programming.Id, new[]
            {
                Q("What does 'HTTP' stand for?", "HyperText Transfer Protocol", "High Transfer Text Protocol", "HyperText Transmission Path", "Home Tool Transfer Protocol"),
                Q("Which data structure uses FIFO ordering?", "Queue", "Stack", "Tree", "Graph"),
                Q("What symbol denotes a comment in C#?", "//", "#", "--", "<!--"),
            }),
        };

        foreach (var (quiz, questions) in quizzes)
        {
            db.Quizzes.Add(quiz);
            db.SaveChanges(); // assigns quiz.Id

            foreach (var (question, options, correctIndex) in questions)
            {
                question.QuizId = quiz.Id;
                question.Points = 10;
                db.Questions.Add(question);
                db.SaveChanges(); // assigns question.Id

                foreach (var opt in options)
                {
                    opt.QuestionId = question.Id;
                    db.Options.Add(opt);
                }
                db.SaveChanges(); // assigns option Ids

                // Now that options have Ids, point the question at the correct one.
                question.CorrectOptionId = options[correctIndex].Id;
                db.SaveChanges();
            }
        }

        // --- Badges ---
        db.Badges.AddRange(
            new Badge { Name = "First Quiz", Description = "Completed your first quiz", Requirement = "first_quiz" },
            new Badge { Name = "Century", Description = "Earned 100 total points", Requirement = "points_100" },
            new Badge { Name = "High Roller", Description = "Earned 500 total points", Requirement = "points_500" },
            new Badge { Name = "Streak Master", Description = "Reached a 7-day streak", Requirement = "streak_7" },
            new Badge { Name = "Perfectionist", Description = "Scored a perfect quiz", Requirement = "perfect_score" }
        );

        // --- Demo users (leaderboard has content; passwords set once auth lands) ---
        db.Users.AddRange(
            new User { Username = "demo", Email = "demo@ts-msa.local", PasswordHash = "", TotalPoints = 0, Level = 0 },
            new User { Username = "alice", Email = "alice@ts-msa.local", PasswordHash = "", TotalPoints = 230, Level = 2 },
            new User { Username = "bob", Email = "bob@ts-msa.local", PasswordHash = "", TotalPoints = 280, Level = 2 }
        );

        db.SaveChanges();
    }

    // Helpers ---------------------------------------------------------------

    private static (Quiz, (Question, Option[], int)[]) BuildQuiz(
        string title, string description, string difficulty, int categoryId,
        (Question, Option[], int)[] questions)
        => (new Quiz { Title = title, Description = description, Difficulty = difficulty, CategoryId = categoryId }, questions);

    // First answer passed is the correct one; options are stored in given order.
    private static (Question, Option[], int) Q(string text, string correct, params string[] wrong)
    {
        var options = new List<Option> { new() { Text = correct } };
        options.AddRange(wrong.Select(w => new Option { Text = w }));
        return (new Question { Text = text }, options.ToArray(), 0);
    }
}
