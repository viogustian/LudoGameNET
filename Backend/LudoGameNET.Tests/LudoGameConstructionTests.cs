using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class LudoGameConstructionTests
{
    [Fact]
    public void Constructor_NullColors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new LudoGame(null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public void Constructor_InvalidPlayerCount_ThrowsArgumentException(int count)
    {
        var colors = Enum.GetValues<PlayerColor>().Take(Math.Min(count, 4)).ToList();
        while (colors.Count < count)
        {
            colors.Add(PlayerColor.Red); // pad with duplicates when count > 4 isn't reachable, but keep safe
        }

        Assert.Throws<ArgumentException>(() => new LudoGame(colors));
    }

    [Fact]
    public void Constructor_DuplicateColors_ThrowsArgumentException()
    {
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Red };
        Assert.Throws<ArgumentException>(() => new LudoGame(colors));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Constructor_ValidPlayerCount_CreatesGameWithExpectedPlayers(int count)
    {
        var colors = Enum.GetValues<PlayerColor>().Take(count).ToList();
        var game = new LudoGame(colors);

        Assert.Equal(count, game.Players.Count);
        Assert.Equal(colors, game.Players.Select(p => p.Color));
        Assert.All(game.Players, p => Assert.Equal(4, p.Pieces.Count));
    }

    [Fact]
    public void Constructor_SetsInitialStateFields()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.Equal(0, game.CurrentPlayerIndex);
        Assert.Equal(0, game.ConsecutiveSixes);
        Assert.Equal(GameState.NotStarted, game.State);
        Assert.NotNull(game.Board);
        Assert.NotNull(game.Paths);
        Assert.NotNull(game.Dice);
    }

    [Fact]
    public void Constructor_UsesInjectedDice_WhenProvided()
    {
        var dice = new Dice();
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue }, dice);

        Assert.Same(dice, game.Dice);
    }

    [Fact]
    public void Constructor_CreatesOwnDice_WhenNoneProvided()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.NotNull(game.Dice);
    }

    [Fact]
    public void CreatePiecesForColor_ReturnsFourDistinctBasePieces()
    {
        var pieces = LudoGame.CreatePiecesForColor(PlayerColor.Green);

        Assert.Equal(4, pieces.Count);
        Assert.Equal(new[] { 0, 1, 2, 3 }, pieces.Select(p => p.Id));
        Assert.All(pieces, p =>
        {
            Assert.Equal(PlayerColor.Green, p.Color);
            Assert.Equal(PieceState.Base, p.State);
            Assert.Null(p.PathIndex);
        });
    }

    [Fact]
    public void StartGame_SetsStateToPlaying()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.Equal(GameState.NotStarted, game.State);
        game.StartGame();
        Assert.Equal(GameState.Playing, game.State);
    }

    [Fact]
    public void GetCurrentPlayer_ReturnsPlayerAtCurrentIndex()
    {
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue };
        var game = new LudoGame(colors);

        Assert.Equal(PlayerColor.Red, game.GetCurrentPlayer().Color);

        game.CurrentPlayerIndex = 2;
        Assert.Equal(PlayerColor.Blue, game.GetCurrentPlayer().Color);
    }

    [Fact]
    public void RollDice_ReturnsValueBetweenOneAndSixAndUpdatesDice()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        for (var i = 0; i < 50; i++)
        {
            var value = game.RollDice();
            Assert.InRange(value, 1, 6);
            Assert.Equal(value, game.Dice.Value);
        }
    }
}
