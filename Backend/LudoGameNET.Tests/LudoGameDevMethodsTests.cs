using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

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

        // All pieces should start in base
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Base));

        game.DevEnterAllPieces(PlayerColor.Red);

        // All pieces should now be on the board at the start square
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

        // Manually move one piece to the board first
        redPlayer.Pieces[0].State = PieceState.OnBoard;
        redPlayer.Pieces[0].PathIndex = 5;

        game.DevEnterAllPieces(PlayerColor.Red);

        // The piece already on the board should stay at its position
        Assert.That(redPlayer.Pieces[0].PathIndex, Is.EqualTo(5));

        // The other three pieces should be at the start
        Assert.That(redPlayer.Pieces[1].PathIndex, Is.EqualTo(0));
        Assert.That(redPlayer.Pieces[2].PathIndex, Is.EqualTo(0));
        Assert.That(redPlayer.Pieces[3].PathIndex, Is.EqualTo(0));
    }

    [Test]
    public void DevFinishAllPieces_MovesAllPiecesToFinishedState()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        // Move all pieces to the board first
        game.DevEnterAllPieces(PlayerColor.Red);

        game.DevFinishAllPieces(PlayerColor.Red);

        // All pieces should be finished
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

        // Move all pieces to the board first
        game.DevEnterAllPieces(PlayerColor.Red);

        game.DevResetPiecesToBase(PlayerColor.Red);

        // All pieces should be back in base
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

        // Path index must be between 0 and TotalPathLength - 2 for on-board pieces
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
    public void DevForcePiece_RemovesPieceFromOldSquare()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        // Move piece to the board at index 5
        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 5);
        var oldSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 5);
        Assert.That(oldSquare.Pieces.Count, Is.EqualTo(1));

        // Move piece to a new location
        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 10);

        // Old square should be empty
        Assert.That(oldSquare.Pieces.Count, Is.EqualTo(0));

        // New square should have the piece
        var newSquare = game.GetSquareAtPathIndex(PlayerColor.Red, 10);
        Assert.That(newSquare.Pieces.Count, Is.EqualTo(1));
    }

    [Test]
    public void DevForcePiece_EndsGameWhenAllPiecesFinished()
    {
        var game = NewPlayingGame(PlayerColor.Red, PlayerColor.Blue);
        var redPlayer = game.Players[0];

        // Move all but one piece to finished state
        for (int i = 0; i < 3; i++)
        {
            game.DevForcePiece(PlayerColor.Red, i, PieceState.Finished, null);
        }

        Assert.That(game.State, Is.EqualTo(GameState.Playing));

        // Finish the last piece
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
        Assert.Throws<ArgumentException>(() => game.DevSetCurrentPlayer(2)); // Only 2 players
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

        // Set Red as current player
        game.DevSetCurrentPlayer(0);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(0));

        // Move all Red pieces to the board
        game.DevEnterAllPieces(PlayerColor.Red);
        var redPlayer = game.Players[0];
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.OnBoard));

        // Move piece 0 to a specific position
        game.DevForcePiece(PlayerColor.Red, 0, PieceState.OnBoard, 15);
        Assert.That(redPlayer.Pieces[0].PathIndex, Is.EqualTo(15));

        // Set consecutive sixes
        game.DevSetConsecutiveSixes(2);
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(2));

        // Finish piece 0
        game.DevForcePiece(PlayerColor.Red, 0, PieceState.Finished, null);
        Assert.That(redPlayer.Pieces[0].State, Is.EqualTo(PieceState.Finished));

        // Reset all pieces back to base
        game.DevResetPiecesToBase(PlayerColor.Red);
        Assert.That(redPlayer.Pieces, Is.All.Matches<IPiece>(p => p.State == PieceState.Base));

        // Switch to Green player
        game.DevSetCurrentPlayer(1);
        Assert.That(game.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(game.ConsecutiveSixes, Is.EqualTo(0)); // Should be reset

        // Finish Green player
        game.DevFinishAllPieces(PlayerColor.Green);
        Assert.That(game.State, Is.EqualTo(GameState.Finished));
    }
}
