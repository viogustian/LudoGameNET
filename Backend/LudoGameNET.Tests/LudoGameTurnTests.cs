using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class LudoGameTurnTests
{
    [Test]
    public void NextTurn_AdvancesToNextPlayer()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue });

        game.NextTurn();

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
    }

    [Test]
    public void NextTurn_WrapsAroundToFirstPlayer()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue });
        game.CurrentPlayerIndex = 2;

        game.NextTurn();

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));
    }

    [Test]
    public void HandleTurnAfterMove_NonSixValue_AlwaysPassesTurnAndResetsSixStreak()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ConsecutiveSixes = 2;

        game.HandleTurnAfterMove(4);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
    }

    [Test]
    public void HandleTurnAfterMove_SixValue_KeepsTurnUntilThirdConsecutiveSix()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.HandleTurnAfterMove(6);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(1));

        game.HandleTurnAfterMove(6);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(2));

        game.HandleTurnAfterMove(6);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
    }

    [Test]
    public void HandleTurnAfterMove_SixAfterNonSix_StartsAFreshStreak()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.HandleTurnAfterMove(3);
        game.HandleTurnAfterMove(6);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(1));
    }
}
