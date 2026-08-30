using LudoGameNET.Api.Models;
using LudoGameNET.Api.Game;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums (PlayerColor, PieceState, GameState, SquareType) as
        // readable strings ("Red", "OnBoard", ...) instead of numbers.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ludo Game API",
        Version = "v1",
        Description = "Web API backend for a Ludo (Parcheesi-style) board game, generated from the provided class diagram."
    });
});

// The LudoGame engine is created on demand (via IGameManager.CreateGame),
// so the manager itself can be a singleton holding the single active game.
builder.Services.AddSingleton<IGameManager, GameManager>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
