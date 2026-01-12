namespace RedisProj.AuthController;

using Microsoft.AspNetCore.Mvc;
using RedisProj.Models;
using StackExchange.Redis;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly IDatabase _redis;

    public LeaderboardController(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }


    [HttpGet("{type}/{date}")]
    public async Task<IActionResult> GetLeaderboardByDate(string type, string date)
    {
        string leaderboardKey = type.ToLower() switch
        {
            "daily" => $"leaderboard:daily:{date}",
            "monthly" => $"leaderboard:monthly:{date}",
            "all-time" => "leaderboard:all-time",
            _ => null
        };

        if (leaderboardKey == null)
        {
            return BadRequest(new { message = "Nevalidan podatak za leaderboard key!" });
        }

        var topPlayers = await _redis.SortedSetRangeByRankWithScoresAsync(leaderboardKey, 0, 9, Order.Descending);
        var result = new List<object>();


        foreach (var player in topPlayers)
        {
            string playerName = player.Element.ToString();
            string playerHashKey = $"player:{playerName}";

            var playerData = await _redis.HashGetAllAsync(playerHashKey);
            var email = playerData.FirstOrDefault(h => h.Name == "Email").Value.ToString();           

            result.Add(new
            {
                PlayerName = playerName,
                Score = player.Score,
                EmailAddress = email
            });
        }
        
        return Ok(result);
    }

    [HttpDelete("clear-scores/{playerName}")]
    public async Task<IActionResult> ClearScores(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            return BadRequest(new { message = "Ime igraca je obavezno polje!" });
        }

        try
        {
            string globalKey = "leaderboard:all-time";
            string dailyKey = $"leaderboard:daily:{DateTime.UtcNow:yyyy-MM-dd}";
            string monthlyKey = $"leaderboard:monthly:{DateTime.UtcNow:yyyy-MM}";

            var leaderboardKeys = new List<string> { globalKey, dailyKey, monthlyKey };

            foreach (var key in leaderboardKeys)
            {
                await _redis.SortedSetRemoveAsync(key, playerName);
            }

            return Ok(new { message = $"Svi rezultati igraca '{playerName}' su obrisani." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Greska pri brisanju skorova: {ex.Message}" });
        }
    }
}
