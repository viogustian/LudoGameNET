using LudoGameNET.Api.Controllers;
using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class DevControllerTests
{
    private Mock<IGameManager> _gameManagerMock = null!;
    private Mock<IWebHostEnvironment> _envMock = null!;
    private Mock<ILogger<DevController>> _loggerMock = null!;
    private DevController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _gameManagerMock = new Mock<IGameManager>();
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.EnvironmentName).Returns("Development");
        // For testing, we assume we're in development. The actual development check is an integration test.
        _loggerMock = new Mock<ILogger<DevController>>();
        _controller = new DevController(_gameManagerMock.Object, _envMock.Object, _loggerMock.Object);
    }

    [Test]
    public void AnyEndpoint_NotDevelopmentEnvironment_ReturnsNotFound()
    {
        // Arrange
        _envMock.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        var result = _controller.GetDiceStatus();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void SetDice_NoGameStarted_ReturnsNotFound()
    {
        // Arrange
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns((LudoGame)null!);

        // Act
        var result = _controller.SetDice(new DevSetDiceRequest { Value = 4 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public void SetDice_InvalidValue_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act - Value > 6
        var result = _controller.SetDice(new DevSetDiceRequest { Value = 7 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());

        // Act - Value < 1
        result = _controller.SetDice(new DevSetDiceRequest { Value = 0 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void SetDice_ValidValue_ReturnsOkWithDiceStatus()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetDice(new DevSetDiceRequest { Value = 4, Lock = false });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var diceStatus = (result.Result as OkObjectResult)!.Value as DevDiceStatusDto;
        Assert.That(diceStatus!.ForcedValue, Is.EqualTo(4));
        Assert.That(diceStatus.Locked, Is.False);
    }

    [Test]
    public void SetDice_WithLock_SetsLockedFlag()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetDice(new DevSetDiceRequest { Value = 5, Lock = true });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var diceStatus = (result.Result as OkObjectResult)!.Value as DevDiceStatusDto;
        Assert.That(diceStatus!.ForcedValue, Is.EqualTo(5));
        Assert.That(diceStatus.Locked, Is.True);
    }

    [Test]
    public void SetDice_WithNullValue_ClearsForce()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ForcedDiceValue = 3;
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetDice(new DevSetDiceRequest { Value = null });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var diceStatus = (result.Result as OkObjectResult)!.Value as DevDiceStatusDto;
        Assert.That(diceStatus!.ForcedValue, Is.Null);
    }

    [Test]
    public void ClearDice_WithGameStarted_ClearsTheValue()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ForcedDiceValue = 4;
        game.DiceLocked = true;
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.ClearDice();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var diceStatus = (result.Result as OkObjectResult)!.Value as DevDiceStatusDto;
        Assert.That(diceStatus!.ForcedValue, Is.Null);
        Assert.That(diceStatus.Locked, Is.False);
    }

    [Test]
    public void GetDiceStatus_WithGameStarted_ReturnsCurrentStatus()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.ForcedDiceValue = 3;
        game.DiceLocked = true;
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.GetDiceStatus();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var diceStatus = (result.Result as OkObjectResult)!.Value as DevDiceStatusDto;
        Assert.That(diceStatus!.ForcedValue, Is.EqualTo(3));
        Assert.That(diceStatus.Locked, Is.True);
    }

    [Test]
    public void EnterAll_WithInvalidColor_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.EnterAll(new DevColorRequest { Color = PlayerColor.Green }); // Not in game

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void EnterAll_WithValidColor_ReturnsOkWithGameState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.EnterAll(new DevColorRequest { Color = PlayerColor.Red });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto, Is.Not.Null);
    }

    [Test]
    public void FinishAll_WithValidColor_EndsGameForPlayer()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.FinishAll(new DevColorRequest { Color = PlayerColor.Red });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto!.State, Is.EqualTo(GameState.Finished));
    }

    [Test]
    public void FinishAll_WithInvalidColor_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.FinishAll(new DevColorRequest { Color = PlayerColor.Green });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void ResetToBase_WithValidColor_ReturnsPiecesToBase()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        game.DevEnterAllPieces(PlayerColor.Red);
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.ResetToBase(new DevColorRequest { Color = PlayerColor.Red });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto, Is.Not.Null);
    }

    [Test]
    public void ResetToBase_WithInvalidColor_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.ResetToBase(new DevColorRequest { Color = PlayerColor.Green });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void ForcePiece_WithValidRequest_ReturnsOkWithGameState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.ForcePiece(new DevForcePieceRequest 
        { 
            Color = PlayerColor.Red, 
            PieceId = 0, 
            State = PieceState.OnBoard, 
            PathIndex = 5 
        });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto, Is.Not.Null);
    }

    [Test]
    public void ForcePiece_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act - Invalid color
        var result = _controller.ForcePiece(new DevForcePieceRequest 
        { 
            Color = PlayerColor.Green, 
            PieceId = 0, 
            State = PieceState.OnBoard, 
            PathIndex = 5 
        });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void SetTurn_WithValidIndex_ChangesCurrentPlayer()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Green, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetTurn(new DevSetTurnRequest { PlayerIndex = 2 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto!.CurrentPlayerIndex, Is.EqualTo(2));
    }

    [Test]
    public void SetTurn_WithInvalidIndex_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetTurn(new DevSetTurnRequest { PlayerIndex = 99 }); // Invalid index

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void SetSixes_WithValidCount_ReturnsOkWithGameState()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetSixes(new DevSetSixesRequest { Count = 2 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var stateDto = (result.Result as OkObjectResult)!.Value as GameStateDto;
        Assert.That(stateDto!.ConsecutiveSixes, Is.EqualTo(2));
    }

    [Test]
    public void SetSixes_WithNegativeCount_ReturnsBadRequest()
    {
        // Arrange
        var game = new LudoGame(new List<PlayerColor> { PlayerColor.Red, PlayerColor.Blue });
        game.StartGame();
        _gameManagerMock.Setup(gm => gm.CurrentGame).Returns(game);

        // Act
        var result = _controller.SetSixes(new DevSetSixesRequest { Count = -1 });

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }
}
