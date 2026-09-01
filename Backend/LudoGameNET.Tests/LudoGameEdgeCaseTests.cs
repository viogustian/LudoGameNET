using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class LudoGameEdgeCaseTests
{
    [Test]
    public void RollDice_WithForcedValueAndDiceNotLocked_ClearsTheForceValue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        // Set forced dice value and ensure DiceLocked is false
        game.ForcedDiceValue = 4;
        game.DiceLocked = false;

        var value = game.RollDice();

        Assert.That(value, Is.EqualTo(4));
        Assert.That(game.ForcedDiceValue, Is.Null); // Should be cleared
    }

    [Test]
    public void RollDice_WithForcedValueAndDiceLocked_DoesNotClearTheForceValue()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });

        // Set forced dice value and lock it
        game.ForcedDiceValue = 5;
        game.DiceLocked = true;

        var value = game.RollDice();

        Assert.That(value, Is.EqualTo(5));
        Assert.That(game.ForcedDiceValue, Is.EqualTo(5)); // Should NOT be cleared
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
        
        // Manually set Board to null
        game.Board = null;

        // When Board is null, IsValidPosition returns false, which throws ArgumentOutOfRangeException
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
        
        // Manually set Paths to null
        game.Paths = null;

        Assert.Throws<InvalidOperationException>(() => game.GetSquareAtPathIndex(PlayerColor.Red, 0));
    }

    [Test]
    public void DevRemoveFromCurrentSquare_WithBasePiece_DoesNotThrow()
    {
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var piece = game.Players[0].Pieces[0];

        // Piece is in base state - should not throw
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

        // Set piece to OnBoard but PathIndex to null
        piece.State = PieceState.OnBoard;
        piece.PathIndex = null;

        // Should not throw - the pattern matching prevents the inner code from running
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

        // Set initial forced value
        game.ForcedDiceValue = 3;
        game.DiceLocked = false;

        var value1 = game.RollDice();
        Assert.That(value1, Is.EqualTo(3));
        Assert.That(game.ForcedDiceValue, Is.Null); // Cleared after first roll

        // Next roll should be random (no forced value)
        var value2 = game.RollDice();
        Assert.That(value2, Is.InRange(1, 6));
    }
}
