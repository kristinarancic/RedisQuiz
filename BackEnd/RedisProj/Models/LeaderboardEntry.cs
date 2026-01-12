using System.ComponentModel.DataAnnotations;

namespace RedisProj.Models;

public class LeaderboardEntry
{   
    [Key]
    public string? PlayerName { get; set; }

    [EmailAddress]
    public required string EmailAddress { get; set; }
    
    public int Score { get; set; }

    public DateTime Date { get; set; }
}