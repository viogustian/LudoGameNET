using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LudoGameNET.Api.Game;
public interface IGameManager
{
    LudoGame? CurrentGame { get; }

    LudoGame CreateGame(List<PlayerColor> colors);
}

public class GameManager : IGameManager
{
    private readonly ILogger<GameManager> _logger;
    private readonly ILogger<LudoGame> _gameLogger;
    private LudoGame? _currentGame;

    public LudoGame? CurrentGame => _currentGame;

    // Loggers are optional (default to no-ops) so GameManager can still be
    // constructed directly with `new GameManager()` in unit tests; ASP.NET
    // Core's DI container supplies the real ILogger<T> instances at runtime.
    public GameManager(ILogger<GameManager>? logger = null, ILogger<LudoGame>? gameLogger = null)
    {
        _logger = logger ?? NullLogger<GameManager>.Instance;
        _gameLogger = gameLogger ?? NullLogger<LudoGame>.Instance;
    }

    public LudoGame CreateGame(List<PlayerColor> colors)
    {
        _logger.LogInformation(
            "Creating a new game for {PlayerCount} players with colors {Colors}",
            colors?.Count, colors);

        var game = new LudoGame(colors!, logger: _gameLogger);
        game.StartGame();
        _currentGame = game;

        _logger.LogInformation("Game created and started. First turn belongs to {PlayerColor}",
            game.GetCurrentPlayer().Color);

        return game;
    }
}
