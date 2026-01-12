namespace RedisProj.Models;

public class LoginRequest
{
    public required string PlayerName { get; set; }
    public required string Email { get; set; }
}