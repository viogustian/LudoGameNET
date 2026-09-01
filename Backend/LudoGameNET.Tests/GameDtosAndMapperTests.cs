using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Mapping;
using LudoGameNET.Api.Models;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class GameDtosTests
{
    [Test]
    public void StartGameRequest_DefaultsColorsToEmptyList()
    {
        // Act
        var request = new StartGameRequest();

        // Assert
        Assert.That(request.Colors, Is.Not.Null);
        Assert.That(request.Colors, Is.Empty);
    }

    [Test]
    public void StartGameRequest_CanSetColors()
    {
        // Arrange
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue };

        // Act
        var request = new StartGameRequest { Colors = colors };

        // Assert
        Assert.That(request.Colors, Is.EqualTo(colors));
        Assert.That(request.Colors.Count, Is.EqualTo(2));
    }

    [Test]
    public void MovePieceRequest_SetsProperties()
    {
        // Act
        var request = new MovePieceRequest { PieceId = 2, DiceValue = 5 };

        // Assert
        Assert.That(request.PieceId, Is.EqualTo(2));
        Assert.That(request.DiceValue, Is.EqualTo(5));
    }

    [Test]
    public void PieceDto_From_ConvertsFromIPiece()
    {
        // Arrange
        var piece = new Piece(1, PlayerColor.Green, PieceState.OnBoard, 7);

        // Act
        var dto = PieceDto.From(piece);

        // Assert
        Assert.That(dto.Id, Is.EqualTo(1));
        Assert.That(dto.Color, Is.EqualTo(PlayerColor.Green));
        Assert.That(dto.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(dto.PathIndex, Is.EqualTo(7));
    }

    [Test]
    public void PieceDto_From_WithNullPathIndex()
    {
        // Arrange
        var piece = new Piece(0, PlayerColor.Red, PieceState.Base, null);

        // Act
        var dto = PieceDto.From(piece);

        // Assert
        Assert.That(dto.PathIndex, Is.Null);
    }

    [Test]
    public void PlayerDto_DefaultsToEmptyPiecesList()
    {
        // Act
        var playerDto = new PlayerDto();

        // Assert
        Assert.That(playerDto.Pieces, Is.Not.Null);
        Assert.That(playerDto.Pieces, Is.Empty);
    }

    [Test]
    public void PlayerDto_CanSetProperties()
    {
        // Arrange
        var piece = new Piece(0, PlayerColor.Blue, PieceState.OnBoard, 5);
        var pieceDto = PieceDto.From(piece);

        // Act
        var playerDto = new PlayerDto
        {
            Id = 1,
            Color = PlayerColor.Blue,
            Pieces = new List<PieceDto> { pieceDto }
        };

        // Assert
        Assert.That(playerDto.Id, Is.EqualTo(1));
        Assert.That(playerDto.Color, Is.EqualTo(PlayerColor.Blue));
        Assert.That(playerDto.Pieces.Count, Is.EqualTo(1));
    }

    [Test]
    public void SquareDto_DefaultsToEmptyPiecesList()
    {
        // Act
        var squareDto = new SquareDto();

        // Assert
        Assert.That(squareDto.Pieces, Is.Not.Null);
        Assert.That(squareDto.Pieces, Is.Empty);
    }

    [Test]
    public void SquareDto_CanSetAllProperties()
    {
        // Arrange
        var piece = new Piece(0, PlayerColor.Red, PieceState.OnBoard, 3);
        var pieceDto = PieceDto.From(piece);

        // Act
        var squareDto = new SquareDto
        {
            Row = 5,
            Column = 7,
            Type = SquareType.Safe,
            HomeColor = PlayerColor.Red,
            Pieces = new List<PieceDto> { pieceDto }
        };

        // Assert
        Assert.That(squareDto.Row, Is.EqualTo(5));
        Assert.That(squareDto.Column, Is.EqualTo(7));
        Assert.That(squareDto.Type, Is.EqualTo(SquareType.Safe));
        Assert.That(squareDto.HomeColor, Is.EqualTo(PlayerColor.Red));
        Assert.That(squareDto.Pieces.Count, Is.EqualTo(1));
    }

    [Test]
    public void GameStateDto_DefaultsToEmptyPlayersList()
    {
        // Act
        var stateDto = new GameStateDto();

        // Assert
        Assert.That(stateDto.Players, Is.Not.Null);
        Assert.That(stateDto.Players, Is.Empty);
    }

    [Test]
    public void GameStateDto_CanSetAllProperties()
    {
        // Act
        var stateDto = new GameStateDto
        {
            State = GameState.Playing,
            CurrentPlayerIndex = 1,
            ConsecutiveSixes = 2,
            LastDiceValue = 4,
            WinnerColor = null
        };

        // Assert
        Assert.That(stateDto.State, Is.EqualTo(GameState.Playing));
        Assert.That(stateDto.CurrentPlayerIndex, Is.EqualTo(1));
        Assert.That(stateDto.ConsecutiveSixes, Is.EqualTo(2));
        Assert.That(stateDto.LastDiceValue, Is.EqualTo(4));
        Assert.That(stateDto.WinnerColor, Is.Null);
    }

    [Test]
    public void RollDiceResponseDto_DefaultsToEmptyValidPiecesList()
    {
        // Act
        var response = new RollDiceResponseDto();

        // Assert
        Assert.That(response.ValidPieces, Is.Not.Null);
        Assert.That(response.ValidPieces, Is.Empty);
    }

    [Test]
    public void RollDiceResponseDto_CanSetAllProperties()
    {
        // Arrange
        var piece = new Piece(0, PlayerColor.Red, PieceState.Base, null);
        var pieceDto = PieceDto.From(piece);

        // Act
        var response = new RollDiceResponseDto
        {
            DiceValue = 6,
            CurrentPlayerIndex = 0,
            ValidPieces = new List<PieceDto> { pieceDto }
        };

        // Assert
        Assert.That(response.DiceValue, Is.EqualTo(6));
        Assert.That(response.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(response.ValidPieces.Count, Is.EqualTo(1));
    }

    [Test]
    public void DevSetDiceRequest_CanSetProperties()
    {
        // Act
        var request = new DevSetDiceRequest { Value = 3, Lock = true };

        // Assert
        Assert.That(request.Value, Is.EqualTo(3));
        Assert.That(request.Lock, Is.True);
    }

    [Test]
    public void DevSetDiceRequest_WithNullValue()
    {
        // Act
        var request = new DevSetDiceRequest { Value = null, Lock = false };

        // Assert
        Assert.That(request.Value, Is.Null);
        Assert.That(request.Lock, Is.False);
    }

    [Test]
    public void DevDiceStatusDto_CanSetAllProperties()
    {
        // Act
        var status = new DevDiceStatusDto
        {
            ForcedValue = 4,
            Locked = true,
            CurrentDiceValue = 2
        };

        // Assert
        Assert.That(status.ForcedValue, Is.EqualTo(4));
        Assert.That(status.Locked, Is.True);
        Assert.That(status.CurrentDiceValue, Is.EqualTo(2));
    }

    [Test]
    public void DevColorRequest_CanSetColor()
    {
        // Act
        var request = new DevColorRequest { Color = PlayerColor.Yellow };

        // Assert
        Assert.That(request.Color, Is.EqualTo(PlayerColor.Yellow));
    }

    [Test]
    public void DevForcePieceRequest_CanSetAllProperties()
    {
        // Act
        var request = new DevForcePieceRequest
        {
            Color = PlayerColor.Blue,
            PieceId = 2,
            State = PieceState.OnBoard,
            PathIndex = 10
        };

        // Assert
        Assert.That(request.Color, Is.EqualTo(PlayerColor.Blue));
        Assert.That(request.PieceId, Is.EqualTo(2));
        Assert.That(request.State, Is.EqualTo(PieceState.OnBoard));
        Assert.That(request.PathIndex, Is.EqualTo(10));
    }

    [Test]
    public void DevSetTurnRequest_CanSetPlayerIndex()
    {
        // Act
        var request = new DevSetTurnRequest { PlayerIndex = 2 };

        // Assert
        Assert.That(request.PlayerIndex, Is.EqualTo(2));
    }

    [Test]
    public void DevSetSixesRequest_CanSetCount()
    {
        // Act
        var request = new DevSetSixesRequest { Count = 3 };

        // Assert
        Assert.That(request.Count, Is.EqualTo(3));
    }
}

[TestFixture]
public class GameStateMapperTests
{
    [Test]
    public void ToGameStateDto_WithPlayingGame_ReturnsCorrectState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.State, Is.EqualTo(GameState.Playing));
        Assert.That(dto.CurrentPlayerIndex, Is.EqualTo(0));
        Assert.That(dto.ConsecutiveSixes, Is.EqualTo(0));
        Assert.That(dto.Players.Count, Is.EqualTo(2));
    }

    [Test]
    public void ToGameStateDto_WithFinishedGame_ReturnsWinner()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        game.DevFinishAllPieces(PlayerColor.Red);

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.State, Is.EqualTo(GameState.Finished));
        Assert.That(dto.WinnerColor, Is.EqualTo(PlayerColor.Red));
    }

    [Test]
    public void ToGameStateDto_WithZeroDiceValue_ReturnsNullForLastDiceValue()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        // Dice value is 0 by default (not rolled yet)

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.LastDiceValue, Is.Null);
    }

    [Test]
    public void ToGameStateDto_WithNonZeroDiceValue_ReturnsLastDiceValue()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        game.RollDice(); // Roll at least once to get a non-zero value

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.LastDiceValue, Is.Not.Null);
        Assert.That(dto.LastDiceValue, Is.InRange(1, 6));
    }

    [Test]
    public void ToPlayerDto_ConvertsPlayerCorrectly()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Green, PlayerColor.Red });
        var player = game.Players[0];

        // Act
        var dto = GameStateMapper.ToPlayerDto(player);

        // Assert
        Assert.That(dto.Id, Is.EqualTo(player.Id));
        Assert.That(dto.Color, Is.EqualTo(PlayerColor.Green));
        Assert.That(dto.Pieces.Count, Is.EqualTo(4));
    }

    [Test]
    public void ToPlayerDto_ConvertsPiecesCorrectly()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Yellow, PlayerColor.Red });
        var player = game.Players[0];

        // Act
        var dto = GameStateMapper.ToPlayerDto(player);

        // Assert
        for (int i = 0; i < player.Pieces.Count; i++)
        {
            Assert.That(dto.Pieces[i].Id, Is.EqualTo(player.Pieces[i].Id));
            Assert.That(dto.Pieces[i].Color, Is.EqualTo(player.Pieces[i].Color));
            Assert.That(dto.Pieces[i].State, Is.EqualTo(player.Pieces[i].State));
            Assert.That(dto.Pieces[i].PathIndex, Is.EqualTo(player.Pieces[i].PathIndex));
        }
    }

    [Test]
    public void ToGameStateDto_WithMultiplePlayers_IncludesAllPlayers()
    {
        // Arrange
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Yellow, PlayerColor.Blue };
        var game = new LudoGame(colors);

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.Players.Count, Is.EqualTo(4));
        for (int i = 0; i < colors.Count; i++)
        {
            Assert.That(dto.Players[i].Color, Is.EqualTo(colors[i]));
        }
    }

    [Test]
    public void ToGameStateDto_WithConsecutiveSixes_IncludesCount()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.DevSetConsecutiveSixes(2);

        // Act
        var dto = GameStateMapper.ToGameStateDto(game);

        // Assert
        Assert.That(dto.ConsecutiveSixes, Is.EqualTo(2));
    }

    [Test]
    public void ToPlayerDto_WithMovedPieces_IncludesPieceState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        game.DevEnterAllPieces(PlayerColor.Red);
        var player = game.Players[0];

        // Act
        var dto = GameStateMapper.ToPlayerDto(player);

        // Assert
        Assert.That(dto.Pieces, Is.All.Matches<PieceDto>(p => p.State == PieceState.OnBoard));
    }
}
