using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class LudoGameBoardAndPathTests
{
    private static LudoGame NewGame(params PlayerColor[] colors) =>
        new(colors.ToList());

    [Test]
    public void CreateBoard_ProducesFullSizeGrid()
    {
        var board = LudoGame.CreateBoard();

        Assert.That(board.Squares.GetLength(0), Is.EqualTo(LudoGame.BoardSize));
        Assert.That(board.Squares.GetLength(1), Is.EqualTo(LudoGame.BoardSize));

        foreach (var square in board.Squares)
        {
            Assert.That(square, Is.Not.Null);
            Assert.That(square.Pieces, Is.Not.Null);
            Assert.That(square.Pieces, Is.Empty);
        }
    }

    [Test]
    public void CreateBoard_MarksExactlyTheKnownSafeSquaresAsSafe()
    {
        var board = LudoGame.CreateBoard();

        var actualSafePoints = new HashSet<Point>();
        for (var r = 0; r < LudoGame.BoardSize; r++)
        {
            for (var c = 0; c < LudoGame.BoardSize; c++)
            {
                if (board.Squares[r, c].Type == SquareType.Safe)
                {
                    actualSafePoints.Add(new Point(r, c));
                }
            }
        }

        Assert.That(actualSafePoints, Is.EquivalentTo(LudoGame.SafeSquares));
    }

    [TestCase(PlayerColor.Red)]
    [TestCase(PlayerColor.Green)]
    [TestCase(PlayerColor.Yellow)]
    [TestCase(PlayerColor.Blue)]
    public void CreateBoard_EachColorsFinalHomeStretchSquareIsItsGoal(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();
        var goalPoint = LudoGame.HomeStretches[color][^1];
        var goalSquare = board.Squares[goalPoint.Row, goalPoint.Column];

        Assert.That(goalSquare.Type, Is.EqualTo(SquareType.Goal));
        Assert.That(goalSquare.HomeColor, Is.EqualTo(color));
    }

    [TestCase(PlayerColor.Red)]
    [TestCase(PlayerColor.Green)]
    [TestCase(PlayerColor.Yellow)]
    [TestCase(PlayerColor.Blue)]
    public void CreateBoard_NonFinalHomeStretchSquaresAreHomeStretchType(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();
        var stretchPoints = LudoGame.HomeStretches[color];

        foreach (var point in stretchPoints.Take(stretchPoints.Count - 1))
        {
            var square = board.Squares[point.Row, point.Column];
            Assert.That(square.Type, Is.EqualTo(SquareType.HomeStretch));
            Assert.That(square.HomeColor, Is.EqualTo(color));
        }
    }

    [TestCase(PlayerColor.Red)]
    [TestCase(PlayerColor.Green)]
    [TestCase(PlayerColor.Yellow)]
    [TestCase(PlayerColor.Blue)]
    public void CreateBoard_YardHoldingPointsBelongToTheirColorsYard(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();

        foreach (var point in LudoGame.YardHoldingPoints[color])
        {
            var square = board.Squares[point.Row, point.Column];
            Assert.That(square.Type, Is.EqualTo(SquareType.Yard));
            Assert.That(square.HomeColor, Is.EqualTo(color));
        }
    }

    [Test]
    public void CreateBoard_CenterSquareIsCommon()
    {
        var board = LudoGame.CreateBoard();
        var center = board.Squares[7, 7];

        Assert.That(center.Type, Is.EqualTo(SquareType.Common));
    }

    [Test]
    public void CreatePaths_BuildsAPathForEveryColor()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.That(game.Paths, Is.Not.Null);
        Assert.That(game.Paths!.Count, Is.EqualTo(Enum.GetValues<PlayerColor>().Length));
        foreach (var path in game.Paths.Values)
        {
            Assert.That(path.Count, Is.EqualTo(LudoGame.TotalPathLength));
        }
    }

    [TestCase(PlayerColor.Red)]
    [TestCase(PlayerColor.Green)]
    [TestCase(PlayerColor.Yellow)]
    [TestCase(PlayerColor.Blue)]
    public void BuildPathForColor_StartsAtTheColorsCommonTrackOffset(PlayerColor color)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var path = game.BuildPathForColor(color);
        var expectedStart = LudoGame.CommonPath[LudoGame.StartOffsets[color]];

        Assert.That(path[0], Is.EqualTo(expectedStart));
    }

    [TestCase(PlayerColor.Red)]
    [TestCase(PlayerColor.Green)]
    [TestCase(PlayerColor.Yellow)]
    [TestCase(PlayerColor.Blue)]
    public void BuildPathForColor_EndsWithTheColorsOwnHomeStretch(PlayerColor color)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var path = game.BuildPathForColor(color);

        Assert.That(path.Skip(LudoGame.CommonTrackLength - 1), Is.EqualTo(LudoGame.HomeStretches[color]));
    }

    [Test]
    public void GetSquare_ValidPosition_ReturnsMatchingBoardSquare()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var point = new Point(6, 1);

        var square = game.GetSquare(point);

        Assert.That(square.Position, Is.EqualTo(point));
    }

    [TestCase(-1, 0)]
    [TestCase(0, -1)]
    [TestCase(15, 0)]
    [TestCase(0, 15)]
    public void GetSquare_OutOfRangePosition_ThrowsArgumentOutOfRangeException(int row, int col)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(row, col)));
    }

    [TestCase(0, 0, true)]
    [TestCase(14, 14, true)]
    [TestCase(-1, 0, false)]
    [TestCase(0, -1, false)]
    [TestCase(15, 0, false)]
    [TestCase(0, 15, false)]
    public void IsValidPosition_ChecksBothAxesAgainstBoardBounds(int row, int col, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.That(game.IsValidPosition(new Point(row, col)), Is.EqualTo(expected));
    }

    [TestCase(SquareType.Safe, true)]
    [TestCase(SquareType.Yard, true)]
    [TestCase(SquareType.HomeStretch, true)]
    [TestCase(SquareType.Goal, true)]
    [TestCase(SquareType.Common, false)]
    public void IsSafePosition_ReflectsSquareType(SquareType type, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var square = new Square(new Point(0, 0), type, PlayerColor.Red, new List<IPiece>());

        Assert.That(game.IsSafePosition(square), Is.EqualTo(expected));
    }
}
