using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class LudoGameMovePieceTests
{
    private static LudoGame NewPlayingGame(params PlayerColor[] colors)
    {
        var game = new LudoGame(colors.ToList());
        game.StartGame();
        return game;
    }

    [Fact]
    public void MovePiece_GameNotPlaying_ThrowsInvalidOperationException()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue }); // not started
        var player = game.Players[0];
        var piece = player.Pieces[0];

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 6));
    }

    [Fact]
    public void MovePiece_PieceNotOwnedByPlayer_ThrowsArgumentException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];
        var blue = game.Players[1];

        Assert.Throws<ArgumentException>(() => game.MovePiece(red, blue.Pieces[0], 6));
    }

    [Fact]
    public void MovePiece_BasePieceWithoutSix_ThrowsInvalidOperationException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 4));
    }

    [Fact]
    public void MovePiece_BasePieceWithSix_EntersBoardAtColorsStartSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];

        game.MovePiece(player, piece, 6);

        Assert.Equal(PieceState.OnBoard, piece.State);
        Assert.Equal(0, piece.PathIndex);

        var startSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 0);
        Assert.Contains(piece, startSquare.Pieces);
    }

    [Fact]
    public void MovePiece_EnteringBoardWithSix_KeepsTurnAndIncrementsConsecutiveSixes()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        game.MovePiece(player, player.Pieces[0], 6);

        Assert.Equal(0, game.CurrentPlayerIndex); // still Red's turn
        Assert.Equal(1, game.ConsecutiveSixes);
    }

    [Fact]
    public void MovePiece_NonSixMove_PassesTurnToNextPlayer()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = 3;

        game.MovePiece(player, piece, 2);

        Assert.Equal(1, game.CurrentPlayerIndex); // turn passed to Blue
        Assert.Equal(0, game.ConsecutiveSixes);
    }

    [Fact]
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

        Assert.Equal(5, piece.PathIndex);
        Assert.DoesNotContain(piece, oldSquare.Pieces);
        var newSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        Assert.Contains(piece, newSquare.Pieces);
    }

    [Fact]
    public void MovePiece_OvershootingMove_ThrowsInvalidOperationException()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = LudoGame.TotalPathLength - 4;

        Assert.Throws<InvalidOperationException>(() => game.MovePiece(player, piece, 6));
    }

    [Fact]
    public void MovePiece_LandingOnNonSafeSquareWithOpponent_CapturesOpponentPiece()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var red = game.Players[0];
        var blue = game.Players[1];

        var mover = red.Pieces[0];
        mover.State = PieceState.OnBoard;
        mover.PathIndex = 3; // Red path index 3 is a Common (non-safe) square

        var opponent = blue.Pieces[0];
        opponent.State = PieceState.OnBoard;
        opponent.PathIndex = 20; // arbitrary, irrelevant to capture logic

        var targetSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5); // Red index 5 is also non-safe
        Assert.Equal(SquareType.Common, targetSquare.Type);
        targetSquare.Pieces.Add(opponent);

        game.MovePiece(red, mover, 2);

        Assert.Equal(PieceState.Base, opponent.State);
        Assert.Null(opponent.PathIndex);
        Assert.DoesNotContain(opponent, targetSquare.Pieces);
        Assert.Contains(mover, targetSquare.Pieces);
    }

    [Fact]
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

        var targetSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 8); // Red index 8 is Safe
        Assert.Equal(SquareType.Safe, targetSquare.Type);
        targetSquare.Pieces.Add(opponent);

        game.MovePiece(red, mover, 2);

        Assert.Equal(PieceState.OnBoard, opponent.State);
        Assert.Contains(opponent, targetSquare.Pieces);
        Assert.Contains(mover, targetSquare.Pieces);
    }

    [Fact]
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

        Assert.Equal(PieceState.OnBoard, teammatePiece.State);
        Assert.Contains(teammatePiece, targetSquare.Pieces);
        Assert.Contains(mover, targetSquare.Pieces);
    }

    [Fact]
    public void MovePiece_ReachingTheLastIndex_MarksPieceFinishedAndDoesNotPlaceItOnASquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];
        var piece = player.Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = LudoGame.TotalPathLength - 4;

        game.MovePiece(player, piece, 3);

        Assert.Equal(PieceState.Finished, piece.State);
        Assert.Equal(LudoGame.TotalPathLength - 1, piece.PathIndex);
    }

    [Fact]
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

        Assert.Equal(PieceState.Finished, lastPiece.State);
        Assert.True(game.CheckWinner(player));
        Assert.Equal(GameState.Finished, game.State);
    }

    [Fact]
    public void CapturePiece_ResetsPieceToBaseAndRemovesFromSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = game.Players[1].Pieces[0];
        piece.State = PieceState.OnBoard;
        piece.PathIndex = 10;

        var square = new Square(new Point(1, 1), SquareType.Common, PlayerColor.Red, new List<IPiece> { piece });

        game.CapturePiece(piece, square);

        Assert.Equal(PieceState.Base, piece.State);
        Assert.Null(piece.PathIndex);
        Assert.DoesNotContain(piece, square.Pieces);
    }

    [Fact]
    public void HandleCapture_SkipsCaptureOnSafeSquares()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var mover = game.Players[0].Pieces[0];
        var opponent = game.Players[1].Pieces[0];
        opponent.State = PieceState.OnBoard;
        opponent.PathIndex = 5;

        var square = new Square(new Point(2, 2), SquareType.Safe, PlayerColor.Red, new List<IPiece> { mover, opponent });

        game.HandleCapture(mover, square);

        Assert.Equal(PieceState.OnBoard, opponent.State);
        Assert.Contains(opponent, square.Pieces);
    }

    [Fact]
    public void CheckWinner_TrueOnlyWhenAllFourPiecesFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        Assert.False(game.CheckWinner(player));

        foreach (var p in player.Pieces)
        {
            p.State = PieceState.Finished;
        }

        Assert.True(game.CheckWinner(player));
    }

    [Fact]
    public void EndGame_SetsStateToFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);

        game.EndGame();

        Assert.Equal(GameState.Finished, game.State);
    }
}
