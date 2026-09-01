using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

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

        Assert.That(game.CanMove(piece, 3), Is.True); // lands exactly on the last index
    }

    [Test]
    public void CanMove_FalseWhenStepsOvershootThePath()
    {
        var game = NewGame(PlayerColor.Red, PlayerColor.Blue);
        var piece = MakePiece(PlayerColor.Red, PieceState.OnBoard, LudoGame.TotalPathLength - 4);

        Assert.That(game.CanMove(piece, 6), Is.False); // would land past the last index
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

        Assert.That(validOnSix.Count, Is.EqualTo(4)); // all 4 base pieces can enter
        Assert.That(validOnFour, Is.Empty); // no base pieces can move without a 6
    }

    [Test]
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
        Assert.That(validPieces.Any(p => p.Id == 1), Is.True);
        Assert.That(validPieces.Any(p => p.Id == 0), Is.False);
        Assert.That(validPieces.Any(p => p.Id == 2), Is.False);
        Assert.That(validPieces.Any(p => p.Id == 3), Is.False);
    }
}
