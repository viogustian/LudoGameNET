using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Mapping;
using LudoGameNET.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudoGameNET.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameManager gameManager, ILogger<GameController> logger)
    {
        _gameManager = gameManager;
        _logger = logger;
    }

    [HttpPost]
    public ActionResult<GameStateDto> StartGame([FromBody] StartGameRequest request)
    {
        try
        {
            var game = _gameManager.CreateGame(request.Colors);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentNullException)
        {
            _logger.LogWarning(ex, "Rejected StartGame request with colors {Colors}", request.Colors);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public ActionResult<GameStateDto> GetState()
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet. POST /api/game to start one." });
        }

        return Ok(GameStateMapper.ToGameStateDto(game));
    }

    [HttpGet("current-player")]
    public ActionResult<PlayerDto> GetCurrentPlayer()
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet." });
        }

        return Ok(GameStateMapper.ToPlayerDto(game.GetCurrentPlayer()));
    }

    [HttpPost("roll")]
    public ActionResult<RollDiceResponseDto> RollDice()
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            _logger.LogWarning("RollDice requested but no game has been started");
            return NotFound(new { error = "No game has been started yet." });
        }

        if (game.State != GameState.Playing)
        {
            _logger.LogWarning("RollDice rejected because the game state is {GameState}", game.State);
            return BadRequest(new { error = "The game is not currently in progress." });
        }

        var diceValue = game.RollDice();
        var currentPlayer = game.GetCurrentPlayer();
        var validPieces = game.GetValidPieces(currentPlayer, diceValue);

        _logger.LogInformation(
            "Player {PlayerId} ({PlayerColor}) rolled {DiceValue} with {ValidPieceCount} valid piece(s) to move",
            currentPlayer.Id, currentPlayer.Color, diceValue, validPieces.Count);

        if (validPieces.Count == 0)
        {
            game.HandleTurnAfterMove(diceValue);
        }

        return Ok(new RollDiceResponseDto
        {
            DiceValue = diceValue,
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            ValidPieces = validPieces.Select(PieceDto.From).ToList(),
        });
    }

    [HttpGet("valid-pieces")]
    public ActionResult<List<PieceDto>> GetValidPieces([FromQuery] int playerId, [FromQuery] int diceValue)
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet." });
        }

        var player = game.Players.FirstOrDefault(p => p.Id == playerId);
        if (player is null)
        {
            return BadRequest(new { error = $"No player with id {playerId}." });
        }

        var validPieces = game.GetValidPieces(player, diceValue);
        return Ok(validPieces.Select(PieceDto.From).ToList());
    }

    [HttpPost("move")]
    public ActionResult<GameStateDto> MovePiece([FromBody] MovePieceRequest request)
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet." });
        }

        var currentPlayer = game.GetCurrentPlayer();
        var piece = currentPlayer.Pieces.FirstOrDefault(p => p.Id == request.PieceId);
        if (piece is null)
        {
            _logger.LogWarning(
                "MovePiece rejected: player {PlayerId} has no piece with id {PieceId}",
                currentPlayer.Id, request.PieceId);
            return BadRequest(new { error = $"Current player has no piece with id {request.PieceId}." });
        }

        try
        {
            game.MovePiece(currentPlayer, piece, request.DiceValue);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _logger.LogWarning(
                ex,
                "MovePiece rejected for player {PlayerId}, piece {PieceId}, dice value {DiceValue}",
                currentPlayer.Id, piece.Id, request.DiceValue);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("board")]
    public ActionResult<List<SquareDto>> GetBoard()
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet." });
        }

        var squares = new List<SquareDto>();
        var boardSquares = game.Board?.Squares;
        if (boardSquares is null)
        {
            return BadRequest(new { error = "Board is not initialized." });
        }

        for (var row = 0; row < boardSquares.GetLength(0); row++)
        {
            for (var col = 0; col < boardSquares.GetLength(1); col++)
            {
                var square = boardSquares[row, col];
                squares.Add(new SquareDto
                {
                    Row = square.Position.Row,
                    Column = square.Position.Column,
                    Type = square.Type,
                    HomeColor = square.HomeColor,
                    Pieces = square.Pieces.Select(PieceDto.From).ToList(),
                });
            }
        }

        return Ok(squares);
    }

    [HttpGet("square")]
    public ActionResult<SquareDto> GetSquare([FromQuery] int row, [FromQuery] int column)
    {
        var game = _gameManager.CurrentGame;
        if (game is null)
        {
            return NotFound(new { error = "No game has been started yet." });
        }

        var position = new Point(row, column);
        if (!game.IsValidPosition(position))
        {
            return BadRequest(new { error = "Position is outside the board." });
        }

        var square = game.GetSquare(position);
        return Ok(new SquareDto
        {
            Row = square.Position.Row,
            Column = square.Position.Column,
            Type = square.Type,
            HomeColor = square.HomeColor,
            Pieces = square.Pieces.Select(PieceDto.From).ToList(),
        });
    }

}
