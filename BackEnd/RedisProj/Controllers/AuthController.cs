namespace RedisProj.AuthController;

using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using RedisProj.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IDatabase _redis;

    public AuthController(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.PlayerName) || string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(new { message = "Polja za email i ime igraca su obavezna!" });
        }

        //Proveravamo da li postoji takav hash koji sadrzi PlayerName kao polje
        string playerHashKey = $"player:{request.PlayerName}";
        var playerExists = await _redis.HashExistsAsync(playerHashKey, "PlayerName");

        if (!playerExists)
        {
            // Ako korisnik ne postoji, kreiraj ga
            await _redis.HashSetAsync(playerHashKey, new HashEntry[]
            {
                new HashEntry("PlayerName", request.PlayerName),
                new HashEntry("Email", request.Email)
            });
        }

        // Dohvati podatke korisnika
        var playerData = await _redis.HashGetAllAsync(playerHashKey);
        var playerName = playerData.FirstOrDefault(h => h.Name == "PlayerName").Value.ToString();
        var email = playerData.FirstOrDefault(h => h.Name == "Email").Value.ToString();

        return Ok(new
        {
            playerName,
            email
        });
    }

    //Ovde brisemo igraca i sve njegove podatke. Ne koristimo za sada.
    [HttpDelete("delete/{playerName}")]
    public async Task<IActionResult> DeletePlayer(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            return BadRequest(new { message = "Polje Ime igraca je obavezno!" });
        }

        string globalKey = "leaderboard:all-time";
        string playerHashKey = $"player:{playerName}";

        // Lista svih vremenskih leaderboard-a
        var allKeys = new List<string>
        {
            globalKey,
            $"leaderboard:daily:{DateTime.UtcNow:yyyy-MM-dd}",
            $"leaderboard:monthly:{DateTime.UtcNow:yyyy-MM}"
        };

        try
        {
            // Obrisi igrača iz svih leaderboard-a
            foreach (var key in allKeys)
            {
                await _redis.SortedSetRemoveAsync(key, playerName);
            }

            // Obrisi sve podatke igrača
            await _redis.KeyDeleteAsync(playerHashKey);

            return Ok(new { message = $"Igrac '{playerName}' je uspesno izbrisan sa svim svojim podacima." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Greska prilikom brisanja igraca: {ex.Message}" });
        }
    }
}