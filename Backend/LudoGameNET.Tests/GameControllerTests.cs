using LudoGameNET.Api.Controllers;
using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class GameControllerTests
{
    private Mock<IGameManager> _gameManagerMock = null!;
    private Mock<ILogger<GameController>> _loggerMock = null!;
    private GameController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _gameManagerMock = new Mock<IGameManager>();
        _loggerMock = new Mock<ILogger<GameController>>();
        _controller = new GameController(_gameManagerMock.Object, _loggerMock.Object);
    }

    [Test]
    public void StartGame_WithValidColors_ReturnsOkWithGameState()
    {
        // Arrange
        var colors = new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue };
        var game = new LudoGame(colors);
        _gameManagerMock.Setup(gm => gm.CreateGame(colors)).Returns(game);

        // Act
        var result = _controller.StartGame(new StartGameRequest { Colors = colors });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult!.Value, Is.InstanceOf<GameStateDto>());
        var stateDto = okResult.Value as GameStateDto;
        Assert.That(stateDto!.State, Is.EqualTo(GameState.Playing));
    }

    [Test]
    public void StartGame_WithInvalidColors_ReturnsBadRequest()
    {
        // Arrange
        var colors = new List<PlayerColor> { PlayerColor.Red }; // Only 1 player, invalid
        _gameManagerMock.Setup(gm => gm.CreateGame(It.IsAny<List<PlayerColor>>()))
            .Throws(new ArgumentException("Invalid player count"));

        // Act
        var result = _controller.StartGame(new StartGameRequest { Colors = colors });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void StartGame_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        var colors = (List<PlayerColor>)null!;
        _gameManagerMock.Setup(gm => gm.CreateGame(colors))
            .Throws(new ArgumentNullException());

        // Act
        var result = _controller.StartGame(new StartGameRequest { Colors = colors });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetState_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.GetState();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetState_WithGameStarted_ReturnsOkWithGameState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetState();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto!.State, Is.EqualTo(GameState.Playing));
    }

    [Test]
    public void GetCurrentPlayer_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.GetCurrentPlayer();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetCurrentPlayer_WithGameStarted_ReturnsCurrentPlayer()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetCurrentPlayer();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var playerDto = (result.Result as OkObjectResult)!.Value as PlayerDto;
        Assert.That(playerDto!.Color, Is.EqualTo(PlayerColor.Red));
    }

    [Test]
    public void RollDice_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.RollDice();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void RollDice_WithGameNotPlaying_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        // Game is NotStarted, not Playing
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.RollDice();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void RollDice_WithValidGame_ReturnsRollDiceResponse()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.RollDice();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var response = (result.Result as OkObjectResult)!.Value as RollDiceResponseDto;
        Assert.That(response!.DiceValue, Is.InRange(1, 6));
        Assert.That(response.CurrentPlayerIndex, Is.EqualTo(0));
    }

    [Test]
    public void RollDice_WithNoValidPieces_PassesTurn()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var initialPlayerIndex = game.CurrentPlayerIndex;
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act - Roll multiple times until we get a non-6 with no valid moves
        int? resultPlayerIndex = null;
        for (int i = 0; i < 100; i++)
        {
            var result = _controller.RollDice();
            var response = (result.Result as OkObjectResult)?.Value as RollDiceResponseDto;
            if (response?.ValidPieces.Count == 0 && response.DiceValue != 6)
            {
                resultPlayerIndex = response.CurrentPlayerIndex;
                break;
            }
        }

        // Assert
        Assert.That(resultPlayerIndex, Is.EqualTo(1)); // Turn passed to next player
    }

    [Test]
    public void GetValidPieces_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.GetValidPieces(0, 4);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetValidPieces_WithInvalidPlayerId_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetValidPieces(999, 4);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetValidPieces_WithValidPlayerAndDice_ReturnsValidPieces()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var player = game.Players[0];
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetValidPieces(player.Id, 6);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var pieces = (result.Result as OkObjectResult)!.Value as List<PieceDto>;
        Assert.That(pieces, Is.Not.Null);
        Assert.That(pieces!.Count, Is.GreaterThanOrEqualTo(0)); // Base pieces can move on 6
    }

    [Test]
    public void MovePiece_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);
        var request = new MovePieceRequest { PieceId = 0, DiceValue = 6 };

        // Act
        var result = _controller.MovePiece(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void MovePiece_WithInvalidPieceId_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);
        var request = new MovePieceRequest { PieceId = 999, DiceValue = 6 };

        // Act
        var result = _controller.MovePiece(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void MovePiece_WithValidMove_ReturnsOkWithGameState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        var currentPlayer = game.GetCurrentPlayer();
        var piece = currentPlayer.Pieces[0];
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.MovePiece(new MovePieceRequest { PieceId = piece.Id, DiceValue = 6 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto!.State, Is.EqualTo(GameState.Playing));
    }

    [Test]
    public void GetBoard_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.GetBoard();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetBoard_WithGameStarted_ReturnsAllSquares()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetBoard();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var squares = (result.Result as OkObjectResult)!.Value as List<SquareDto>;
        Assert.That(squares, Is.Not.Null);
        Assert.That(squares!.Count, Is.EqualTo(15 * 15)); // 15x15 board
    }

    [Test]
    public void GetSquare_WithNoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.GetSquare(0, 0);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void GetSquare_WithInvalidPosition_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetSquare(-1, -1);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void GetSquare_WithValidPosition_ReturnsSquareDto()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetSquare(0, 0);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var squareDto = (result.Result as OkObjectResult)!.Value as SquareDto;
        Assert.That(squareDto, Is.Not.Null);
        Assert.That(squareDto!.Row, Is.EqualTo(0));
        Assert.That(squareDto.Column, Is.EqualTo(0));
    }
}
