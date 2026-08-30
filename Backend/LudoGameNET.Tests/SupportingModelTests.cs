using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class PieceTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var piece = new Piece(2, PlayerColor.Yellow, PieceState.OnBoard, 7);

        Assert.Equal(2, piece.Id);
        Assert.Equal(PlayerColor.Yellow, piece.Color);
        Assert.Equal(PieceState.OnBoard, piece.State);
        Assert.Equal(7, piece.PathIndex);
    }

    [Fact]
    public void StateAndPathIndex_AreMutable()
    {
        var piece = new Piece(0, PlayerColor.Red, PieceState.Base, null);

        piece.State = PieceState.OnBoard;
        piece.PathIndex = 0;

        Assert.Equal(PieceState.OnBoard, piece.State);
        Assert.Equal(0, piece.PathIndex);
    }
}

public class PlayerTests
{
    [Fact]
    public void Constructor_SetsIdColorAndPieces()
    {
        var pieces = LudoGame.CreatePiecesForColor(PlayerColor.Blue);
        var player = new Player(1, PlayerColor.Blue, pieces);

        Assert.Equal(1, player.Id);
        Assert.Equal(PlayerColor.Blue, player.Color);
        Assert.Same(pieces, player.Pieces);
    }

    [Fact]
    public void Constructor_NullPieces_DefaultsToEmptyList()
    {
        var player = new Player(0, PlayerColor.Red, null!);

        Assert.NotNull(player.Pieces);
        Assert.Empty(player.Pieces);
    }
}

public class DiceTests
{
    [Fact]
    public void Value_DefaultsToZeroAndIsSettable()
    {
        var dice = new Dice();

        Assert.Equal(0, dice.Value);

        dice.Value = 5;

        Assert.Equal(5, dice.Value);
    }
}

public class PointTests
{
    [Fact]
    public void Equality_IsBasedOnRowAndColumn()
    {
        var a = new Point(3, 4);
        var b = new Point(3, 4);
        var c = new Point(4, 3);

        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
        Assert.True(a == b);
        Assert.False(a == c);
    }

    [Fact]
    public void CanBeUsedAsADictionaryKey()
    {
        var dict = new Dictionary<Point, string>
        {
            [new Point(1, 2)] = "value",
        };

        Assert.Equal("value", dict[new Point(1, 2)]);
    }
}

public class SquareTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var piece = new Piece(0, PlayerColor.Green, PieceState.OnBoard, 4);
        var square = new Square(new Point(1, 7), SquareType.HomeStretch, PlayerColor.Green, new List<IPiece> { piece });

        Assert.Equal(new Point(1, 7), square.Position);
        Assert.Equal(SquareType.HomeStretch, square.Type);
        Assert.Equal(PlayerColor.Green, square.HomeColor);
        Assert.Contains(piece, square.Pieces);
    }

    [Fact]
    public void Constructor_NullPieces_DefaultsToEmptyList()
    {
        var square = new Square(new Point(0, 0), SquareType.Common, PlayerColor.Red, null!);

        Assert.NotNull(square.Pieces);
        Assert.Empty(square.Pieces);
    }
}

public class BoardTests
{
    [Fact]
    public void Constructor_StoresProvidedSquaresGrid()
    {
        var squares = new Square[2, 2];
        for (var r = 0; r < 2; r++)
        for (var c = 0; c < 2; c++)
            squares[r, c] = new Square(new Point(r, c), SquareType.Common, PlayerColor.Red, new List<IPiece>());

        var board = new Board(squares);

        Assert.Same(squares, board.Squares);
    }
}

public class GameManagerTests
{
    [Fact]
    public void CurrentGame_IsNullBeforeAnyGameIsCreated()
    {
        var manager = new GameManager();

        Assert.Null(manager.CurrentGame);
    }

    [Fact]
    public void CreateGame_StartsTheGameAndExposesItAsCurrentGame()
    {
        var manager = new GameManager();

        var game = manager.CreateGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.Equal(GameState.Playing, game.State);
        Assert.Same(game, manager.CurrentGame);
    }

    [Fact]
    public void CreateGame_CalledAgain_ReplacesThePreviousGame()
    {
        var manager = new GameManager();
        var first = manager.CreateGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        var second = manager.CreateGame(new List<PlayerColor> { PlayerColor.Green, PlayerColor.Yellow });

        Assert.Same(second, manager.CurrentGame);
        Assert.NotSame(first, manager.CurrentGame);
    }

    [Fact]
    public void CreateGame_InvalidColors_ThrowsAndDoesNotCreateGame()
    {
        var manager = new GameManager();

        Assert.Throws<ArgumentException>(() => manager.CreateGame(new List<PlayerColor> { PlayerColor.Red }));
        Assert.Null(manager.CurrentGame);
    }
}
