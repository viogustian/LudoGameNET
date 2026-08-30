using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class LudoGameBoardAndPathTests
{
    private static LudoGame NewGame(params PlayerColor[] colors) =>
        new(colors.ToList());

    [Fact]
    public void CreateBoard_ProducesFullSizeGrid()
    {
        var board = LudoGame.CreateBoard();

        Assert.Equal(LudoGame.BoardSize, board.Squares.GetLength(0));
        Assert.Equal(LudoGame.BoardSize, board.Squares.GetLength(1));

        foreach (var square in board.Squares)
        {
            Assert.NotNull(square);
            Assert.NotNull(square.Pieces);
            Assert.Empty(square.Pieces);
        }
    }

    [Fact]
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

        Assert.Equal(LudoGame.SafeSquares, actualSafePoints);
    }

    [Theory]
    [InlineData(PlayerColor.Red)]
    [InlineData(PlayerColor.Green)]
    [InlineData(PlayerColor.Yellow)]
    [InlineData(PlayerColor.Blue)]
    public void CreateBoard_EachColorsFinalHomeStretchSquareIsItsGoal(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();
        var goalPoint = LudoGame.HomeStretches[color][^1];
        var goalSquare = board.Squares[goalPoint.Row, goalPoint.Column];

        Assert.Equal(SquareType.Goal, goalSquare.Type);
        Assert.Equal(color, goalSquare.HomeColor);
    }

    [Theory]
    [InlineData(PlayerColor.Red)]
    [InlineData(PlayerColor.Green)]
    [InlineData(PlayerColor.Yellow)]
    [InlineData(PlayerColor.Blue)]
    public void CreateBoard_NonFinalHomeStretchSquaresAreHomeStretchType(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();
        var stretchPoints = LudoGame.HomeStretches[color];

        foreach (var point in stretchPoints.Take(stretchPoints.Count - 1))
        {
            var square = board.Squares[point.Row, point.Column];
            Assert.Equal(SquareType.HomeStretch, square.Type);
            Assert.Equal(color, square.HomeColor);
        }
    }

    [Theory]
    [InlineData(PlayerColor.Red)]
    [InlineData(PlayerColor.Green)]
    [InlineData(PlayerColor.Yellow)]
    [InlineData(PlayerColor.Blue)]
    public void CreateBoard_YardHoldingPointsBelongToTheirColorsYard(PlayerColor color)
    {
        var board = LudoGame.CreateBoard();

        foreach (var point in LudoGame.YardHoldingPoints[color])
        {
            var square = board.Squares[point.Row, point.Column];
            Assert.Equal(SquareType.Yard, square.Type);
            Assert.Equal(color, square.HomeColor);
        }
    }

    [Fact]
    public void CreateBoard_CenterSquareIsCommon()
    {
        var board = LudoGame.CreateBoard();
        var center = board.Squares[7, 7];

        Assert.Equal(SquareType.Common, center.Type);
    }

    [Fact]
    public void CreatePaths_BuildsAPathForEveryColor()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.NotNull(game.Paths);
        Assert.Equal(Enum.GetValues<PlayerColor>().Length, game.Paths!.Count);
        Assert.All(game.Paths.Values, path => Assert.Equal(LudoGame.TotalPathLength, path.Count));
    }

    [Theory]
    [InlineData(PlayerColor.Red)]
    [InlineData(PlayerColor.Green)]
    [InlineData(PlayerColor.Yellow)]
    [InlineData(PlayerColor.Blue)]
    public void BuildPathForColor_StartsAtTheColorsCommonTrackOffset(PlayerColor color)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var path = game.BuildPathForColor(color);
        var expectedStart = LudoGame.CommonPath[LudoGame.StartOffsets[color]];

        Assert.Equal(expectedStart, path[0]);
    }

    [Theory]
    [InlineData(PlayerColor.Red)]
    [InlineData(PlayerColor.Green)]
    [InlineData(PlayerColor.Yellow)]
    [InlineData(PlayerColor.Blue)]
    public void BuildPathForColor_EndsWithTheColorsOwnHomeStretch(PlayerColor color)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var path = game.BuildPathForColor(color);

        Assert.Equal(LudoGame.HomeStretches[color], path.Skip(LudoGame.CommonTrackLength - 1));
    }

    [Fact]
    public void GetSquare_ValidPosition_ReturnsMatchingBoardSquare()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var point = new Point(6, 1);

        var square = game.GetSquare(point);

        Assert.Equal(point, square.Position);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(15, 0)]
    [InlineData(0, 15)]
    public void GetSquare_OutOfRangePosition_ThrowsArgumentOutOfRangeException(int row, int col)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(row, col)));
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(14, 14, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(15, 0, false)]
    [InlineData(0, 15, false)]
    public void IsValidPosition_ChecksBothAxesAgainstBoardBounds(int row, int col, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Equal(expected, game.IsValidPosition(new Point(row, col)));
    }

    [Theory]
    [InlineData(SquareType.Safe, true)]
    [InlineData(SquareType.Yard, true)]
    [InlineData(SquareType.HomeStretch, true)]
    [InlineData(SquareType.Goal, true)]
    [InlineData(SquareType.Common, false)]
    public void IsSafePosition_ReflectsSquareType(SquareType type, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var square = new Square(new Point(0, 0), type, PlayerColor.Red, new List<IPiece>());

        Assert.Equal(expected, game.IsSafePosition(square));
    }
}
