using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class PieceTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var piece = new Piece(2, PlayerColor.Yellow, PieceState.OnBoard, 7);

        Assert.That(piece.Id, Is.EqualTo(2));
        Assert.That(piece.Color, Is.EqualTo(PlayerColor.Yellow));
        Assert.That(piece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(piece.PathIndex, Is.EqualTo(7));
    }

    [Test]
    public void StateAndPathIndex_AreMutable()
    {
        var piece = new Piece(0, PlayerColor.Red, PieceState.Base, null);

        piece.State = PieceState.OnBoard;
        piece.PathIndex = 0;

        Assert.That(piece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(piece.PathIndex, Is.EqualTo(0));
    }
}

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Constructor_SetsIdColorAndPieces()
    {
        var pieces = LudoGame.CreatePiecesForColor(PlayerColor.Blue);
        var player = new Player(1, PlayerColor.Blue, pieces);

        Assert.That(player.Id, Is.EqualTo(1));
        Assert.That(player.Color, Is.EqualTo(PlayerColor.Blue));
        Assert.That(player.Pieces, Is.SameAs(pieces));
    }

    [Test]
    public void Constructor_NullPieces_DefaultsToEmptyList()
    {
        var player = new Player(0, PlayerColor.Red, null!);

        Assert.That(player.Pieces, Is.Not.Null);
        Assert.That(player.Pieces, Is.Empty);
    }
}

[TestFixture]
public class DiceTests
{
    [Test]
    public void Value_DefaultsToZeroAndIsSettable()
    {
        var dice = new Dice();

        Assert.That(dice.Value, Is.EqualTo(0));

        dice.Value = 5;

        Assert.That(dice.Value, Is.EqualTo(5));
    }
}

[TestFixture]
public class PointTests
{
    [Test]
    public void Equality_IsBasedOnRowAndColumn()
    {
        var a = new Point(3, 4);
        var b = new Point(3, 4);
        var c = new Point(4, 3);

        Assert.That(a, Is.EqualTo(b));
        Assert.That(a, Is.Not.EqualTo(c));
        Assert.That(a == b, Is.True);
        Assert.That(a == c, Is.False);
    }

    [Test]
    public void CanBeUsedAsADictionaryKey()
    {
        var dict = new Dictionary<Point, string>
        {
            [new Point(1, 2)] = "value",
        };

        Assert.That(dict[new Point(1, 2)], Is.EqualTo("value"));
    }
}

[TestFixture]
public class SquareTests
{
    [Test]
    public void Constructor_SetsAllProperties()
    {
        var piece = new Piece(0, PlayerColor.Green, PieceState.OnBoard, 4);
        var square = new Square(new Point(1, 7), SquareType.HomeStretch, PlayerColor.Green, new List<IPiece> { piece });

        Assert.That(square.Position, Is.EqualTo(new Point(1, 7)));
        Assert.That(square.Type, Is.EqualTo(SquareType.HomeStretch));
        Assert.That(square.HomeColor, Is.EqualTo(PlayerColor.Green));
        Assert.That(square.Pieces, Does.Contain(piece));
    }

    [Test]
    public void Constructor_NullPieces_DefaultsToEmptyList()
    {
        var square = new Square(new Point(0, 0), SquareType.Common, PlayerColor.Red, null!);

        Assert.That(square.Pieces, Is.Not.Null);
        Assert.That(square.Pieces, Is.Empty);
    }
}

[TestFixture]
public class BoardTests
{
    [Test]
    public void Constructor_StoresProvidedSquaresGrid()
    {
        var squares = new Square[2, 2];
        for (var r = 0; r < 2; r++)
        for (var c = 0; c < 2; c++)
            squares[r, c] = new Square(new Point(r, c), SquareType.Common, PlayerColor.Red, new List<IPiece>());

        var board = new Board(squares);

        Assert.That(board.Squares, Is.SameAs(squares));
    }
}

[TestFixture]
public class GameManagerTests
{
    [Test]
    public void CurrentGame_IsNullBeforeAnyGameIsCreated()
    {
        var manager = new GameManager();

        Assert.That(manager.CurrentGame, Is.Null);
    }

    [Test]
    public void CreateGame_StartsTheGameAndExposesItAsCurrentGame()
    {
        var manager = new GameManager();

        var game = manager.CreateGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.State, Is.EqualTo(GameState.Playing));
        Assert.That(manager.CurrentGame, Is.SameAs(game));
    }

    [Test]
    public void CreateGame_CalledAgain_ReplacesThePreviousGame()
    {
        var manager = new GameManager();
        var first = manager.CreateGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        var second = manager.CreateGame(new List<PlayerColor> { PlayerColor.Green, PlayerColor.Yellow });

        Assert.That(manager.CurrentGame, Is.SameAs(second));
        Assert.That(manager.CurrentGame, Is.Not.SameAs(first));
    }

    [Test]
    public void CreateGame_InvalidColors_ThrowsAndDoesNotCreateGame()
    {
        var manager = new GameManager();

        Assert.Throws<ArgumentException>(() => manager.CreateGame(new List<PlayerColor> { PlayerColor.Red }));
        Assert.That(manager.CurrentGame, Is.Null);
    }
}
