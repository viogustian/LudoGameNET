using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class LudoGameTurnTests
{
    [Fact]
    public void NextTurn_AdvancesToNextPlayer()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue });

        game.NextTurn();

        Assert.Equal(1, game.CurrentPlayerIndex);
    }

    [Fact]
    public void NextTurn_WrapsAroundToFirstPlayer()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue });
        game.CurrentPlayerIndex = 2; // last player

        game.NextTurn();

        Assert.Equal(0, game.CurrentPlayerIndex);
    }

    [Fact]
    public void HandleTurnAfterMove_NonSixValue_AlwaysPassesTurnAndResetsSixStreak()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ConsecutiveSixes = 2;

        game.HandleTurnAfterMove(4);

        Assert.Equal(1, game.CurrentPlayerIndex);
        Assert.Equal(0, game.ConsecutiveSixes);
    }

    [Fact]
    public void HandleTurnAfterMove_SixValue_KeepsTurnUntilThirdConsecutiveSix()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.HandleTurnAfterMove(6);
        Assert.Equal(0, game.CurrentPlayerIndex);
        Assert.Equal(1, game.ConsecutiveSixes);

        game.HandleTurnAfterMove(6);
        Assert.Equal(0, game.CurrentPlayerIndex);
        Assert.Equal(2, game.ConsecutiveSixes);

        // Third consecutive six forfeits the turn (a Ludo house rule against endless sixes).
        game.HandleTurnAfterMove(6);
        Assert.Equal(1, game.CurrentPlayerIndex);
        Assert.Equal(0, game.ConsecutiveSixes);
    }

    [Fact]
    public void HandleTurnAfterMove_SixAfterNonSix_StartsAFreshStreak()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.HandleTurnAfterMove(3); // resets/passes turn
        game.HandleTurnAfterMove(6); // first six of a new streak, for Blue now

        Assert.Equal(1, game.CurrentPlayerIndex); // Blue still has the turn
        Assert.Equal(1, game.ConsecutiveSixes);
    }
}
