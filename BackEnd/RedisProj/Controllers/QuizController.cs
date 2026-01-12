namespace RedisProj.AuthController;

using Microsoft.AspNetCore.Mvc;
using RedisProj.Models;
using StackExchange.Redis;

[ApiController]
[Route("api/quiz")]
public class QuizController : ControllerBase
{
    private readonly IDatabase _redis;

    public QuizController(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }


    [HttpGet("question/{id}")]
    public async Task<IActionResult> GetQuestion(int id)
    {
        var questionKey = $"trivia:question:{id}";

        var question = await _redis.HashGetAsync(questionKey, "question");

        if (!question.HasValue)
            return NotFound(new { message = "Pitanje nije pronadjeno" });

        return Ok(new { id, question = question.ToString() });
    }

    // Endpoint za proveru odgovora
    [HttpPost("answer")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request)
    {
        var questionKey = $"trivia:question:{request.QuestionId}";
        var correctAnswer = await _redis.HashGetAsync(questionKey, "answer");

        if (!correctAnswer.HasValue)
            return NotFound(new { message = "Pitanje nije pronadjeno!" });

        bool isCorrect = request.Answer!.Equals(correctAnswer.ToString(), StringComparison.OrdinalIgnoreCase);
        int points = isCorrect ? 10 : 0;

        return Ok(new
        {
            isCorrect,
            pointsAwarded = points,
            correctAnswer = correctAnswer.ToString()
        });
    }

    [HttpPost("add-question")]
    public async Task<IActionResult> AddQuestion([FromBody] QuestionRequest request)
    {
        if (string.IsNullOrEmpty(request.Question) || string.IsNullOrEmpty(request.Answer))
            return BadRequest(new { message = "Polja za pitanje i odgovor ne mogu biti prazna!" });

        var questionKey = $"trivia:question:{request.Id}";

        await _redis.HashSetAsync(questionKey, new HashEntry[]
        {
            new HashEntry("question", request.Question),
            new HashEntry("answer", request.Answer)
        });

        return Ok(new { message = "Pitanje je uspesno dodato!", id = request.Id });
    }

    // Endpoint za završetak kviza i ažuriranje konačnog rezultata
    [HttpPost("submit-final-score")]
    public async Task<IActionResult> SubmitFinalScore([FromBody] FinalScoreRequest request)
    {
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        var currentMonth = currentDate.Substring(0, 7);
        
        if (string.IsNullOrEmpty(request.PlayerName))
            return BadRequest(new { message = "Polje Ime igraca je obavezno." });

        string[] leaderboardKeys = { "leaderboard:all-time", $"leaderboard:daily:{currentDate}", $"leaderboard:monthly:{currentMonth}" };

        foreach (var leaderboardKey in leaderboardKeys)
        {
            var currentScore = await _redis.SortedSetScoreAsync(leaderboardKey, request.PlayerName);

            if (currentScore == null || request.TotalScore > currentScore)
            {
                await _redis.SortedSetAddAsync(leaderboardKey, request.PlayerName, request.TotalScore);
            }
        }

        return Ok(new { message = "Uspesno dodavanje konacnog rezultata." });
    }
}