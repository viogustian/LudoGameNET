using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class LudoGameConstructionTests
{
    [Test]
    public void Constructor_NullColors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new LudoGame(null!));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(5)]
    public void Constructor_InvalidPlayerCount_ThrowsArgumentException(int count)
    {
        var colors = Enum.GetValues<PlayerColor>().Take(Math.Min(count, 4)).ToList();
        while (colors.Count < count)
        {
            colors.Add(PlayerColor.Red); // pad with duplicates when count > 4 isn't reachable, but keep safe
        }

        Assert.Throws<ArgumentException>(() => new LudoGame(colors));
    }

    [Test]
    public void Constructor_DuplicateColors_ThrowsArgumentException()
    {
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Red };
        Assert.Throws<ArgumentException>(() => new LudoGame(colors));
    }

    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void Constructor_ValidPlayerCount_CreatesGameWithExpectedPlayers(int count)
    {
        var colors = Enum.GetValues<PlayerColor>().Take(count).ToList();
        var game = new LudoGame(colors);

        Assert.That(game.Players.Count, Is.EqualTo(count));
        Assert.That(game.Players.Select(p => p.Color), Is.EqualTo(colors));
        foreach (var p in game.Players)
        {
            Assert.That(p.Pieces.Count, Is.EqualTo(4));
        }
    }

    [Test]
    public void Constructor_SetsInitialStateFields()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
        Assert.That(game.State, Is.EqualTo(GameState.NotStarted));
        Assert.That(game.Board, Is.Not.Null);
        Assert.That(game.Paths, Is.Not.Null);
        Assert.That(game.Dice, Is.Not.Null);
    }

    [Test]
    public void Constructor_UsesInjectedDice_WhenProvided()
    {
        var dice = new Dice();
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue }, dice);

        Assert.That(game.Dice, Is.SameAs(dice));
    }

    [Test]
    public void Constructor_CreatesOwnDice_WhenNoneProvided()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.Dice, Is.Not.Null);
    }

    [Test]
    public void CreatePiecesForColor_ReturnsFourDistinctBasePieces()
    {
        var pieces = LudoGame.CreatePiecesForColor(PlayerColor.Green);

        Assert.That(pieces.Count, Is.EqualTo(4));
        Assert.That(pieces.Select(p => p.Id), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        foreach (var p in pieces)
        {
            Assert.That(p.Color, Is.EqualTo(PlayerColor.Green));
            Assert.That(p.State, Is.EqualTo(PieceState.Base));
            Assert.That(p.PathIndex, Is.Null);
        }
    }

    [Test]
    public void StartGame_SetsStateToPlaying()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.State, Is.EqualTo(GameState.NotStarted));
        game.StartGame();
        Assert.That(game.State, Is.EqualTo(GameState.Playing));
    }

    [Test]
    public void GetCurrentPlayer_ReturnsPlayerAtCurrentIndex()
    {
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue };
        var game = new LudoGame(colors);

        Assert.That(game.GetCurrentPlayer().Color, Is.EqualTo(PlayerColor.Red));

        game.CurrentPlayerIndex = 2;
        Assert.That(game.GetCurrentPlayer().Color, Is.EqualTo(PlayerColor.Blue));
    }

    [Test]
    public void RollDice_ReturnsValueBetweenOneAndSixAndUpdatesDice()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        for (var i = 0; i < 50; i++)
        {
            var value = game.RollDice();
            Assert.That(value, Is.InRange(1, 6));
            Assert.That(game.Dice.Value, Is.EqualTo(value));
        }
    }
}
