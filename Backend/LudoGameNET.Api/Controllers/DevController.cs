using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Mapping;
using LudoGameNET.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudoGameNET.Api.Controllers;

[ApiController]
[Route("api/game/dev")]
public class DevController : ControllerBase
{
    private readonly IGameManager _gameManager;
    private readonly IWebHostEnvironment _env;

    public DevController(IGameManager gameManager, IWebHostEnvironment env)
    {
        _gameManager = gameManager;
        _env = env;
    }

    private bool TryGetGame(out LudoGame game, out ActionResult? error)
    {
        if (!_env.IsDevelopment())
        {
            game = null!;
            error = NotFound(new { error = "Dev tools are only available when the API runs in the Development environment." });
            return false;
        }

        var current = _gameManager.CurrentGame;
        if (current is null)
        {
            game = null!;
            error = NotFound(new { error = "No game has been started yet." });
            return false;
        }

        game = current;
        error = null;
        return true;
    }

    [HttpPost("dice")]
    public ActionResult<DevDiceStatusDto> SetDice([FromBody] DevSetDiceRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        if (request.Value is int v && (v < 1 || v > 6))
        {
            return BadRequest(new { error = "value must be between 1 and 6 (or null to clear)." });
        }

        game.ForcedDiceValue = request.Value;
        game.DiceLocked = request.Value.HasValue && request.Lock;

        return Ok(new DevDiceStatusDto
        {
            ForcedValue = game.ForcedDiceValue,
            Locked = game.DiceLocked,
            CurrentDiceValue = game.Dice.Value == 0 ? null : game.Dice.Value,
        });
    }

    [HttpPost("dice/clear")]
    public ActionResult<DevDiceStatusDto> ClearDice()
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        game.ForcedDiceValue = null;
        game.DiceLocked = false;

        return Ok(new DevDiceStatusDto
        {
            ForcedValue = null,
            Locked = false,
            CurrentDiceValue = game.Dice.Value == 0 ? null : game.Dice.Value,
        });
    }

    [HttpGet("dice")]
    public ActionResult<DevDiceStatusDto> GetDiceStatus()
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        return Ok(new DevDiceStatusDto
        {
            ForcedValue = game.ForcedDiceValue,
            Locked = game.DiceLocked,
            CurrentDiceValue = game.Dice.Value == 0 ? null : game.Dice.Value,
        });
    }

    [HttpPost("enter-all")]
    public ActionResult<GameStateDto> EnterAll([FromBody] DevColorRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevEnterAllPieces(request.Color);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("finish-all")]
    public ActionResult<GameStateDto> FinishAll([FromBody] DevColorRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevFinishAllPieces(request.Color);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public ActionResult<GameStateDto> ResetToBase([FromBody] DevColorRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevResetPiecesToBase(request.Color);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("force-piece")]
    public ActionResult<GameStateDto> ForcePiece([FromBody] DevForcePieceRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevForcePiece(request.Color, request.PieceId, request.State, request.PathIndex);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("set-turn")]
    public ActionResult<GameStateDto> SetTurn([FromBody] DevSetTurnRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevSetCurrentPlayer(request.PlayerIndex);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("set-sixes")]
    public ActionResult<GameStateDto> SetSixes([FromBody] DevSetSixesRequest request)
    {
        if (!TryGetGame(out var game, out var error)) return error!;

        try
        {
            game.DevSetConsecutiveSixes(request.Count);
            return Ok(GameStateMapper.ToGameStateDto(game));
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
