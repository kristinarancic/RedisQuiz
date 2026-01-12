using System.ComponentModel.DataAnnotations;

namespace RedisProj.Models;

public class QuestionRequest
{
    [Key]
    public int Id { get; set; }
    public required string Question { get; set; }
    public required string Answer { get; set; }
}

public class AnswerRequest
{
    public int QuestionId { get; set; }
    public string? PlayerName { get; set; }
    public string? Answer { get; set; }
}

public class FinalScoreRequest
{
    public string? PlayerName { get; set; }
    public int TotalScore { get; set; }
}