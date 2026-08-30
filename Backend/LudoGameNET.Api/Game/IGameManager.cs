using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;

namespace LudoGameNET.Api.Game;
public interface IGameManager
{
    LudoGame? CurrentGame { get; }

    LudoGame CreateGame(List<PlayerColor> colors);
}

public class GameManager : IGameManager
{
    private LudoGame? _currentGame;

    public LudoGame? CurrentGame => _currentGame;

    public LudoGame CreateGame(List<PlayerColor> colors)
    {
        var game = new LudoGame(colors);
        game.StartGame();
        _currentGame = game;
        return game;
    }
}
