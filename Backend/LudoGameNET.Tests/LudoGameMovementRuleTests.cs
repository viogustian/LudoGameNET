using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using Xunit;

namespace LudoGameNET.Tests;

public class LudoGameMovementRuleTests
{
    private static LudoGame NewGame(params PlayerColor[] colors) =>
        new(colors.ToList());

    private static IPiece MakePiece(PlayerColor color, PieceState state, int? pathIndex, int id = 0) =>
        new Piece(id, color, state, pathIndex);

    [Theory]
    [InlineData(6, true)]
    [InlineData(1, false)]
    [InlineData(5, false)]
    public void CanEnterBoard_OnlyTrueForBasePieceWithSix(int diceValue, bool expected)
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.Base, null);

        Assert.Equal(expected, game.CanEnterBoard(piece, diceValue));
    }

    [Fact]
    public void CanEnterBoard_FalseWhenPieceIsNotInBase()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, 3);

        Assert.False(game.CanEnterBoard(piece, 6));
    }

    [Fact]
    public void CanMove_FalseForPieceNotOnBoard()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var basePiece = MakePiece(PlayerColor.Red, PieceState.Base, null);
        var finishedPiece = MakePiece(PlayerColor.Red, PieceState.Finished, LudoGame.TotalPathLength - 1);

        Assert.False(game.CanMove(basePiece, 3));
        Assert.False(game.CanMove(finishedPiece, 3));
    }

    [Fact]
    public void CanMove_TrueWhenStepsStayWithinPath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 4);

        Assert.True(game.CanMove(piece, 3)); // lands exactly on the last index
    }

    [Fact]
    public void CanMove_FalseWhenStepsOvershootThePath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 4);

        Assert.False(game.CanMove(piece, 6)); // would land past the last index
    }

    [Fact]
    public void GetNextPathIndex_AddsStepsToCurrentIndex_TreatingNullAsZero()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var freshPiece = MakePiece(PlayerColor.Red, PieceState.Base, null);
        var onBoardPiece = MakePiece(PlayerColor.Red, PieceState.OnBoard, 10);

        Assert.Equal(4, game.GetNextPathIndex(freshPiece, 4));
        Assert.Equal(16, game.GetNextPathIndex(onBoardPiece, 6));
    }

    [Fact]
    public void HasReachedFinish_TrueWhenStateIsFinishedOrAtLastIndex()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var finishedByState = MakePiece(PlayerColor.Red, PieceState.Finished, 3);
        var finishedByIndex = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 1);
        var midPath = MakePiece(PlayerColor.Red, PieceState.OnBoard, 5);

        Assert.True(game.HasReachedFinish(finishedByState));
        Assert.True(game.HasReachedFinish(finishedByIndex));
        Assert.False(game.HasReachedFinish(midPath));
    }

    [Fact]
    public void GetSquareAtPathIndex_ReturnsTheSquareOnThatColorsPath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var expectedPoint = game.Paths![PlayerColor.Red][5];

        var square = game.GetSquareAtPathIndex(PlayerColor.Red, 5);

        Assert.Equal(expectedPoint, square.Position);
    }

    [Fact]
    public void GetValidPieces_IncludesBasePiecesOnlyOnSix()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        var validOnSix = game.GetValidPieces(player, 6);
        var validOnFour = game.GetValidPieces(player, 4);

        Assert.Equal(4, validOnSix.Count); // all 4 base pieces can enter
        Assert.Empty(validOnFour); // no base pieces can move without a 6
    }

    [Fact]
    public void GetValidPieces_IncludesOnBoardPiecesThatCanLegallyMove()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var player = game.Players[0];

        // One piece near the end of its path, one freshly entered, two still in base.
        player.Pieces[0].State = PieceState.OnBoard;
        player.Pieces[0].PathIndex = LudoGame.TotalPathLength - 2;
        player.Pieces[1].State = PieceState.OnBoard;
        player.Pieces[1].PathIndex = 0;

        var validPieces = game.GetValidPieces(player, 3);

        // Piece 0 would overshoot (needs <=1 to finish), piece 1 can move fine, base pieces need a 6.
        Assert.Contains(validPieces, p => p.Id == 1);
        Assert.DoesNotContain(validPieces, p => p.Id == 0);
        Assert.DoesNotContain(validPieces, p => p.Id == 2);
        Assert.DoesNotContain(validPieces, p => p.Id == 3);
    }
}
