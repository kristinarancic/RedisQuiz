using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Konfigurisanje Redis konekcije kao Singleton
var muxer = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { { "redis-11118.c339.eu-west-3-1.ec2.redns.redis-cloud.com", 11118 } },
    User = "default",
    Password = "fAJ4aJYEKgCVSkLbpvGf5zlo32whzNeF"
});

builder.Services.AddSingleton<IConnectionMultiplexer>(muxer);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()  // Dozvoli sve izvore
              .AllowAnyMethod()  // Dozvoli sve HTTP metode (GET, POST, itd.)
              .AllowAnyHeader(); // Dozvoli sve zaglavlja
    });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Testiranje Redis konekcije unutar aplikacije (opciono)
app.MapGet("/test-redis", async (IConnectionMultiplexer connection) =>
{
    try
    {
        var db = connection.GetDatabase();
        db.StringSet("test-key", "test-value");
        var value = db.StringGet("test-key");
        return Results.Ok($"Test connection successful. Retrieved value: {value}");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Connection failed: {ex.Message}");
    }
});

app.UseCors();
app.Run();
