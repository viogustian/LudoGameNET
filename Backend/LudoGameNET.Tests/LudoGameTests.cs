using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

// ============================================================================
// BOARD AND PATH TESTS
// ============================================================================

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

// ============================================================================
// CONSTRUCTION TESTS
// ============================================================================

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
            colors.Add(PlayerColor.Red);
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

// ============================================================================
// MOVEMENT RULE TESTS
// ============================================================================

[TestFixture]
public class LudoGameMovementRuleTests
{
    private static LudoGame NewGame(params PlayerColor[] colors) =>
        new(colors.ToList());

    private static IPiece MakePiece(PlayerColor color, PieceState state, int? pathIndex, int id = 0) =>
        new Piece(id, color, state, pathIndex);

    [TestCase(6, true)]
    [TestCase(1, false)]
    [TestCase(5, false)]
    public void CanEnterBoard_OnlyTrueForBasePieceWithSix(int diceValue, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.Base, null);

        Assert.That(game.CanEnterBoard(piece, diceValue), Is.EqualTo(expected));
    }

    [Test]
    public void CanEnterBoard_FalseWhenPieceIsNotInBase()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, 3);

        Assert.That(game.CanEnterBoard(piece, 6), Is.False);
    }

    [Test]
    public void CanMove_FalseForPieceNotOnBoard()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var basePiece = MakePiece(PlayerColor.Red, PieceState.Base, null);
        var finishedPiece = MakePiece(PlayerColor.Red, PieceState.Finished, LudoGame.TotalPathLength - 1);

        Assert.That(game.CanMove(basePiece, 3), Is.False);
        Assert.That(game.CanMove(finishedPiece, 3), Is.False);
    }

    [Test]
    public void CanMove_TrueWhenStepsStayWithinPath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 4);

        Assert.That(game.CanMove(piece, 3), Is.True);
    }

    [Test]
    public void CanMove_FalseWhenStepsOvershootThePath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 4);

        Assert.That(game.CanMove(piece, 6), Is.False);
    }

    [Test]
    public void GetNextPathIndex_AddsStepsToCurrentIndex_TreatingNullAsZero()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var freshPiece = MakePiece(PlayerColor.Red, PieceState.Base, null);
        var onBoardPiece = MakePiece(PlayerColor.Red, PieceState.OnBoard, 10);

        Assert.That(game.GetNextPathIndex(freshPiece, 4), Is.EqualTo(4));
        Assert.That(game.GetNextPathIndex(onBoardPiece, 6), Is.EqualTo(16));
    }

    [Test]
    public void HasReachedFinish_TrueWhenStateIsFinishedOrAtLastIndex()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var finishedByState = MakePiece(PlayerColor.Red, PieceState.Finished, 3);
        var finishedByIndex = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 1);
        var midPath = MakePiece(PlayerColor.Red, PieceState.OnBoard, 5);

        Assert.That(game.HasReachedFinish(finishedByState), Is.True);
        Assert.That(game.HasReachedFinish(finishedByIndex), Is.True);
        Assert.That(game.HasReachedFinish(midPath), Is.False);
    }

    [Test]
    public void GetSquareAtPathIndex_ReturnsTheSquareOnThatColorsPath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var expectedPoint = game.Paths![PlayerColor.Red][5];

        var square = game.GetSquareAtPathIndex(PlayerColor.Red, 5);

        Assert.That(square.Position, Is.EqualTo(expectedPoint));
    }

    [Test]
    public void GetValidPieces_IncludesBasePiecesOnlyOnSix()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        var validOnSix = game.GetValidPieces(player, 6);
        var validOnFour = game.GetValidPieces(player, 4);

        Assert.That(validOnSix.Count, Is.EqualTo(4));
        Assert.That(validOnFour, Is.Empty);
    }

    [Test]
    public void GetValidPieces_IncludesOnBoardPiecesThatCanLegallyMove()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        player.Pieces[0].State = PieceState.OnBoard;
        player.Pieces[0].PathIndex = LudoGame.TotalPathLength - 2;
        player.Pieces[1].State = PieceState.OnBoard;
        player.Pieces[1].PathIndex = 0;

        var validPieces = game.GetValidPieces(player, 3);

        Assert.That(validPieces.Any(p => p.Id == 1), Is.True);
        Assert.That(validPieces.Any(p => p.Id == 0), Is.False);
        Assert.That(validPieces.Any(p => p.Id == 2), Is.False);
        Assert.That(validPieces.Any(p => p.Id == 3), Is.False);
    }
}

// ============================================================================
// MOVE PIECE TESTS
// ============================================================================

[TestFixture]
public class LudoGameMovePieceTests
{
    private static LudoGame NewPlayingGame(params PlayerColor[] colors)
    {
        var game = new LudoGame(colors.ToList());
        game.StartGame();
        return game;
    }

    [Test]
    public void MovePiece_GameNotPlaying_ThrowsInvalidOperationException()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        var player = game.Players[0];
        var piece = player.Pieces[0];

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 6));
    }

    [Test]
    public void MovePiece_PieceNotOwnedByPlayer_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];
        var blue = game.Players[1];

        Assert.Throws<ArgumentException>(() => game.MovePiece(red, blue.Pieces[0], 6));
    }

    [Test]
    public void MovePiece_BasePieceWithoutSix_ThrowsInvalidOperationException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 4));
    }

    [Test]
    public void MovePiece_BasePieceWithSix_EntersBoardAtColorsStartSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];

        game.MovePiece(player, piece, 6);

        Assert.That(piece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(piece.PathIndex, Is.EqualTo(0));

        var startSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 0);
        Assert.That(startSquare.Pieces, Does.Contain(piece));
    }

    [Test]
    public void MovePiece_EnteringBoardWithSix_KeepsTurnAndIncrementsConsecutiveSixes()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        game.MovePiece(player, player.Pieces[0], 6);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(1));
    }

    [Test]
    public void MovePiece_NonSixMove_PassesTurnToNextPlayer()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = 3;

        game.MovePiece(player, piece, 2);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
    }

    [Test]
    public void MovePiece_OnBoardPiece_MovesToNewSquareAndLeavesOldOne()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = 3;

        var oldSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 3);
        oldSquare.Pieces.Add(piece);

        game.MovePiece(player, piece, 2);

        Assert.That(piece.PathIndex, Is.EqualTo(5));
        Assert.That(oldSquare.Pieces, Does.Not.Contain(piece));
        var newSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        Assert.That(newSquare.Pieces, Does.Contain(piece));
    }

    [Test]
    public void MovePiece_OvershootingMove_ThrowsInvalidOperationException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = LudoGame.TotalPathLength - 4;

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 6));
    }

    [Test]
    public void MovePiece_LandingOnNonSafeSquareWithOpponent_CapturesOpponentPiece()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];
        var blue = game.Players[1];

        var mover = red.Pieces[0];
        mover.State = PieceState.OnBoard;
        mover.PathIndex = 3;

        var opponent = blue.Pieces[0];
        opponent.State = PieceState.OnBoard;
        opponent.PathIndex = 20;

        var targetSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        Assert.That(targetSquare.Type, Is.EqualTo(SquareType.Common));
        targetSquare.Pieces.Add(opponent);

        game.MovePiece(red, mover, 2);

        Assert.That(opponent.State, Is.EqualTo(PieceState.Base));
        Assert.That(opponent.PathIndex, Is.Null);
        Assert.That(targetSquare.Pieces, Does.Not.Contain(opponent));
        Assert.That(targetSquare.Pieces, Does.Contain(mover));
    }

    [Test]
    public void MovePiece_LandingOnSafeSquareWithOpponent_DoesNotCapture()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];
        var blue = game.Players[1];

        var mover = red.Pieces[0];
        mover.State = PieceState.OnBoard;
        mover.PathIndex = 6;

        var opponent = blue.Pieces[0];
        opponent.State = PieceState.OnBoard;
        opponent.PathIndex = 12;

        var targetSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 8);
        Assert.That(targetSquare.Type, Is.EqualTo(SquareType.Safe));
        targetSquare.Pieces.Add(opponent);

        game.MovePiece(red, mover, 2);

        Assert.That(opponent.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(targetSquare.Pieces, Does.Contain(opponent));
        Assert.That(targetSquare.Pieces, Does.Contain(mover));
    }

    [Test]
    public void MovePiece_OwnColorPiecesOnSameSquare_AreNeverCaptured()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];

        var mover = red.Pieces[0];
        mover.State = PieceState.OnBoard;
        mover.PathIndex = 3;

        var teammatePiece = red.Pieces[1];
        teammatePiece.State = PieceState.OnBoard;
        teammatePiece.PathIndex = 5;

        var targetSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        targetSquare.Pieces.Add(teammatePiece);

        game.MovePiece(red, mover, 2);

        Assert.That(teammatePiece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(targetSquare.Pieces, Does.Contain(teammatePiece));
        Assert.That(targetSquare.Pieces, Does.Contain(mover));
    }

    [Test]
    public void MovePiece_ReachingTheLastIndex_MarksPieceFinishedAndDoesNotPlaceItOnASquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = LudoGame.TotalPathLength - 4;

        game.MovePiece(player, piece, 3);

        Assert.That(piece.State, Is.EqualTo(PieceState.Finished));
        Assert.That(piece.PathIndex, Is.EqualTo(LudoGame.TotalPathLength - 1));
    }

    [Test]
    public void MovePiece_LastPieceFinishing_EndsTheGameForThatPlayer()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        foreach (var p in player.Pieces.Take(3))
        {
            p.State = PieceState.Finished;
            p.PathIndex = LudoGame.TotalPathLength - 1;
        }

        var lastPiece = player.Pieces[3];
        lastPiece.State = PieceState.OnBoard;
        lastPiece.PathIndex = LudoGame.TotalPathLength - 2;

        game.MovePiece(player, lastPiece, 1);

        Assert.That(lastPiece.State, Is.EqualTo(PieceState.Finished));
        Assert.That(game.CheckWinner(player), Is.True);
        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }

    [Test]
    public void CapturePiece_ResetsPieceToBaseAndRemovesFromSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = game.Players[1].Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = 10;

        var square = new Square(new Point(1, 1), SquareType.Common, PlayerColor.Red, new List<IPiece> { piece });

        game.CapturePiece(piece, square);

        Assert.That(piece.State, Is.EqualTo(PieceState.Base));
        Assert.That(piece.PathIndex, Is.Null);
        Assert.That(square.Pieces, Does.Not.Contain(piece));
    }

    [Test]
    public void HandleCapture_SkipsCaptureOnSafeSquares()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var mover = game.Players[0].Pieces[0];
        var opponent = game.Players[1].Pieces[0];
        opponent.State = PieceState.OnBoard;
        opponent.PathIndex = 5;

        var square = new Square(new Point(2, 2), SquareType.Safe, PlayerColor.Red, new List<IPiece> { mover, opponent });

        game.HandleCapture(mover, square);

        Assert.That(opponent.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(square.Pieces, Does.Contain(opponent));
    }

    [Test]
    public void CheckWinner_TrueOnlyWhenAllFourPiecesFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        Assert.That(game.CheckWinner(player), Is.False);

        foreach (var p in player.Pieces)
        {
            p.State = PieceState.Finished;
        }

        Assert.That(game.CheckWinner(player), Is.True);
    }

    [Test]
    public void EndGame_SetsStateToFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        game.EndGame();

        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }
}

// ============================================================================
// TURN TESTS
// ============================================================================

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

// ============================================================================
// DEV METHODS TESTS
// ============================================================================

[TestFixture]
public class LudoGameDevMethodsTests
{
    private static LudoGame NewPlayingGame(params PlayerColor[] colors)
    {
        var game = new LudoGame(colors.ToList());
        game.StartGame();
        return game;
    }

    [Test]
    public void DevEnterAllPieces_MovesAllBasePiecesToStartSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Base));

        game.DevEnterAllPieces(PlayerColor.Red);

        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.OnBoard));
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.PathIndex == 0));

        var startSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 0);
        Assert.That(startSquare.Pieces.Count, Is.EqualTo(4));
    }

    [Test]
    public void DevEnterAllPieces_InvalidColor_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => game.DevEnterAllPieces(PlayerColor.Green));
    }

    [Test]
    public void DevEnterAllPieces_OnlyMovesBasePieces()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        redPlayer.Pieces[0].State = PieceState.OnBoard;
        redPlayer.Pieces[0].PathIndex = 5;

        game.DevEnterAllPieces(PlayerColor.Red);

        Assert.That(redPlayer.Pieces[0].PathIndex, Is.EqualTo(5));
        Assert.That(redPlayer.Pieces[1].PathIndex, Is.EqualTo(0));
        Assert.That(redPlayer.Pieces[2].PathIndex, Is.EqualTo(0));
        Assert.That(redPlayer.Pieces[3].PathIndex, Is.EqualTo(0));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_RemovesOnBoardPieceFromSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];
        var piece = redPlayer.Pieces[0];

        piece.State = PieceState.OnBoard;
        piece.PathIndex = 5;
        var square = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        square.Pieces.Add(piece);

        Assert.That(square.Pieces, Does.Contain(piece));

        var method = typeof(LudoGame).GetMethod("DevRemoveFromCurrentSquare", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(game, new object[] { piece });

        Assert.That(square.Pieces, Does.Not.Contain(piece));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_DoesNotRemoveBasePiece()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];
        var piece = redPlayer.Pieces[0];

        Assert.That(piece.State, Is.EqualTo(PieceState.Base));

        var method = typeof(LudoGame).GetMethod("DevRemoveFromCurrentSquare", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.DoesNotThrow(() => method?.Invoke(game, new object[] { piece }));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_DoesNotRemoveFinishedPiece()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];
        var piece = redPlayer.Pieces[0];

        piece.State = PieceState.Finished;
        piece.PathIndex = LudoGame.TotalPathLength - 1;

        var method = typeof(LudoGame).GetMethod("DevRemoveFromCurrentSquare", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.DoesNotThrow(() => method?.Invoke(game, new object[] { piece }));
    }

    [Test]
    public void DevFinishAllPieces_MovesAllPiecesToFinishedState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevEnterAllPieces(PlayerColor.Red);

        game.DevFinishAllPieces(PlayerColor.Red);

        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Finished));
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.PathIndex == LudoGame.TotalPathLength - 1));
    }

    [Test]
    public void DevFinishAllPieces_EndsGameWhenPlayerWins()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        game.DevFinishAllPieces(PlayerColor.Red);

        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }

    [Test]
    public void DevFinishAllPieces_InvalidColor_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => game.DevFinishAllPieces(PlayerColor.Green));
    }

    [Test]
    public void DevResetPiecesToBase_MovesAllPiecesToBaseState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevEnterAllPieces(PlayerColor.Red);

        game.DevResetPiecesToBase(PlayerColor.Red);

        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Base));
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.PathIndex == null));
    }

    [Test]
    public void DevResetPiecesToBase_RemovesPiecesFromSquares()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevEnterAllPieces(PlayerColor.Red);
        var startSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 0);
        var pieceCountBefore = startSquare.Pieces.Count;

        game.DevResetPiecesToBase(PlayerColor.Red);

        Assert.That(startSquare.Pieces.Count, Is.EqualTo(0));
    }

    [Test]
    public void DevResetPiecesToBase_InvalidColor_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => game.DevResetPiecesToBase(PlayerColor.Green));
    }

    [Test]
    public void DevForcePiece_MovesPieceToBaseState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.Base, null);

        var piece = redPlayer.Pieces[0];
        Assert.That(piece.State, Is.EqualTo(PieceState.Base));
        Assert.That(piece.PathIndex, Is.Null);
    }

    [Test]
    public void DevForcePiece_MovesPieceToOnBoardState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 5);

        var piece = redPlayer.Pieces[0];
        Assert.That(piece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(piece.PathIndex, Is.EqualTo(5));
    }

    [Test]
    public void DevForcePiece_MovesPieceToOnBoardState_DefaultsPathIndexToZero()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, null);

        var piece = redPlayer.Pieces[0];
        Assert.That(piece.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(piece.PathIndex, Is.EqualTo(0));
    }

    [Test]
    public void DevForcePiece_MovesPieceToFinishedState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.Finished, null);

        var piece = redPlayer.Pieces[0];
        Assert.That(piece.State, Is.EqualTo(PieceState.Finished));
        Assert.That(piece.PathIndex, Is.EqualTo(LudoGame.TotalPathLength - 1));
    }

    [Test]
    public void DevForcePiece_InvalidPathIndex_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => 
            game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, LudoGame.TotalPathLength + 1));

        Assert.Throws<ArgumentException>(() => 
            game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, -1));
    }

    [Test]
    public void DevForcePiece_InvalidColor_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => 
            game.DevForcePiece(PlayerColor.Green, 0, PieceState.Base, null));
    }

    [Test]
    public void DevForcePiece_InvalidPieceId_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => 
            game.DevForcePiece(PlayerColor.Red, 999, PieceState.Base, null));
    }

    [Test]
    public void DevForcePiece_UnknownPieceState_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        var invalidState = (PieceState)999;

        Assert.Throws<ArgumentException>(() => 
            game.DevForcePiece(PlayerColor.Red, 0, invalidState, null));
    }

    [Test]
    public void DevForcePiece_RemovesPieceFromOldSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 5);
        var oldSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        Assert.That(oldSquare.Pieces.Count, Is.EqualTo(1));

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 10);

        Assert.That(oldSquare.Pieces.Count, Is.EqualTo(0));

        var newSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 10);
        Assert.That(newSquare.Pieces.Count, Is.EqualTo(1));
    }

    [Test]
    public void DevForcePiece_EndsGameWhenAllPiecesFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        for (int i = 0; i < 3; i++)
        {
            game.DevForcePiece(PlayerColor.Red, i, PieceState.Finished, null);
        }

        Assert.That(game.State, Is.EqualTo(GameState.Playing));

        game.DevForcePiece(PlayerColor.Red, 3, PieceState.Finished, null);

        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }

    [Test]
    public void DevSetCurrentPlayer_ChangesCurrentPlayer()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));

        game.DevSetCurrentPlayer(1);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));

        game.DevSetCurrentPlayer(2);

        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(2));
    }

    [Test]
    public void DevSetCurrentPlayer_ResetsSixStreak()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        game.ConsecutiveSixes = 2;

        game.DevSetCurrentPlayer(1);

        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
    }

    [Test]
    public void DevSetCurrentPlayer_InvalidIndex_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => game.DevSetCurrentPlayer(-1));
        Assert.Throws<ArgumentException>(() => game.DevSetCurrentPlayer(2));
    }

    [Test]
    public void DevSetConsecutiveSixes_SetsTheValue()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        game.DevSetConsecutiveSixes(0);
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));

        game.DevSetConsecutiveSixes(1);
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(1));

        game.DevSetConsecutiveSixes(3);
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(3));
    }

    [Test]
    public void DevSetConsecutiveSixes_NegativeCount_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        Assert.Throws<ArgumentException>(() => game.DevSetConsecutiveSixes(-1));
    }

    [Test]
    public void DevSetConsecutiveSixes_AllowsZero()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        game.ConsecutiveSixes = 3;

        game.DevSetConsecutiveSixes(0);

        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));
    }

    [Test]
    public void DevMethods_WorkTogether_ComplexScenario()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue);

        game.DevSetCurrentPlayer(0);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));

        game.DevEnterAllPieces(PlayerColor.Red);
        var redPlayer = game.Players[0];
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.OnBoard));

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 15);
        Assert.That(redPlayer.Pieces[0].PathIndex, Is.EqualTo(15));

        game.DevSetConsecutiveSixes(2);
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(2));

        game.DevForcePiece(PlayerColor.Red, 0, PieceState.Finished, null);
        Assert.That(redPlayer.Pieces[0].State, Is.EqualTo(PieceState.Finished));

        game.DevResetPiecesToBase(PlayerColor.Red);
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Base));

        game.DevSetCurrentPlayer(1);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0));

        game.DevFinishAllPieces(PlayerColor.Green);
        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }
}

// ============================================================================
// EDGE CASE TESTS
// ============================================================================

[TestFixture]
public class LudoGameEdgeCaseTests
{
    [Test]
    public void RollDice_WithForcedValueAndDiceNotLocked_ClearsTheForceValue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.ForcedDiceValue = 4;
        game.DiceLocked = false;

        var value = game.RollDice();

        Assert.That(value, Is.EqualTo(4));
        Assert.That(game.ForcedDiceValue, Is.Null);
    }

    [Test]
    public void RollDice_WithForcedValueAndDiceLocked_DoesNotClearTheForceValue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.ForcedDiceValue = 5;
        game.DiceLocked = true;

        var value = game.RollDice();

        Assert.That(value, Is.EqualTo(5));
        Assert.That(game.ForcedDiceValue, Is.EqualTo(5));
    }

    [Test]
    public void RollDice_WithoutForcedValue_ReturnsRandomValue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ForcedDiceValue = null;

        var value = game.RollDice();

        Assert.That(value, Is.InRange(1, 6));
        Assert.That(game.Dice.Value, Is.EqualTo(value));
    }

    [Test]
    public void GetSquare_WithNullBoard_ThrowsArgumentOutOfRangeException()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        
        game.Board = null;

        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(0, 0)));
    }

    [Test]
    public void GetSquare_WithInvalidPosition_ThrowsArgumentOutOfRangeException()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(-1, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(0, -1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(15, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => game.GetSquare(new Point(0, 15)));
    }

    [Test]
    public void GetSquareAtPathIndex_WithNullPaths_ThrowsInvalidOperationException()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        
        game.Paths = null;

        Assert.Throws<InvalidOperationException>(() => game.GetSquareAtPathIndex(PlayerColor.Red, 0));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_WithBasePiece_DoesNotThrow()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var piece = game.Players[0].Pieces[0];

        var method = typeof(LudoGame).GetMethod("DevRemoveFromCurrentSquare",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.DoesNotThrow(() => method?.Invoke(game, new object[] { piece }));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_WithNullPathIndex_DoesNotThrow()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var piece = game.Players[0].Pieces[0];

        piece.State = PieceState.OnBoard;
        piece.PathIndex = null;

        var method = typeof(LudoGame).GetMethod("DevRemoveFromCurrentSquare",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.DoesNotThrow(() => method?.Invoke(game, new object[] { piece }));
    }

    [Test]
    public void IsValidPosition_WithNullBoard_ReturnsFalse()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.Board = null;

        Assert.That(game.IsValidPosition(new Point(0, 0)), Is.False);
        Assert.That(game.IsValidPosition(new Point(5, 5)), Is.False);
    }

    [Test]
    public void IsValidPosition_WithValidPosition_ReturnsTrue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.IsValidPosition(new Point(0, 0)), Is.True);
        Assert.That(game.IsValidPosition(new Point(7, 7)), Is.True);
        Assert.That(game.IsValidPosition(new Point(14, 14)), Is.True);
    }
 
    [Test]
    public void IsValidPosition_WithInvalidPosition_ReturnsFalse()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        Assert.That(game.IsValidPosition(new Point(-1, 0)), Is.False);
        Assert.That(game.IsValidPosition(new Point(0, -1)), Is.False);
        Assert.That(game.IsValidPosition(new Point(15, 0)), Is.False);
        Assert.That(game.IsValidPosition(new Point(0, 15)), Is.False);
    }

    [Test]
    public void RollDice_MultipleTimes_UpdatesDiceEachTime()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        for (int i = 0; i < 10; i++)
        {
            var value = game.RollDice();
            Assert.That(game.Dice.Value, Is.EqualTo(value));
            Assert.That(value, Is.InRange(1, 6));
        }
    }

    [Test]
    public void RollDice_WithMultipleForcedValues_UsesOnlyTheCurrentOne()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        game.ForcedDiceValue = 3;
        game.DiceLocked = false;

        var value1 = game.RollDice();
        Assert.That(value1, Is.EqualTo(3));
        Assert.That(game.ForcedDiceValue, Is.Null);

        var value2 = game.RollDice();
        Assert.That(value2, Is.InRange(1, 6));
    }
}

// ============================================================================
// SUPPORTING MODEL TESTS
// ============================================================================

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
