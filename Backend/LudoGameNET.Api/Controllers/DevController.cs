using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Mapping;
using LudoGameNET.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace LudoGameNET.Api.Controllers;

/// <summary>
/// Debug-only endpoints backing the frontend's DevTools panel: forcing dice
/// rolls, teleporting pieces, sending everything to Goal, jumping turns, etc.
/// Every gameplay rule is bypassed on purpose — this exists purely to make
/// edge cases reproducible for manual testing.
///
/// Every action here 404s unless the API is running in the Development
/// environment (the default when you just `dotnet run` locally), so this
/// surface can never be reached from a real deployment.
/// </summary>
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

    /// <summary>Forces the next dice roll(s) to return a specific value. Pass
    /// value: null to clear it and go back to normal random rolls.</summary>
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

    /// <summary>Clears any forced dice value, going back to normal random rolls.</summary>
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

    /// <summary>Reads the current forced-dice status (used by the frontend to
    /// show what the next roll will produce).</summary>
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

    /// <summary>Sends every Base piece of a color onto the board at once, bypassing the "must roll a 6" rule.</summary>
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

    /// <summary>Sends every piece of a color straight to the Goal. Ends the game (declares that color the winner) if it completes the player.</summary>
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

    /// <summary>Resets every piece of a color back to Base (yard) — the reverse of enter-all, for re-running a scenario from scratch.</summary>
    [HttpPost("reset-base")]
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

    /// <summary>Generic "teleport one piece anywhere" endpoint — forces a single piece into an arbitrary state/path index. The building block for any edge case that doesn't have a dedicated shortcut above.</summary>
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

    /// <summary>Jumps straight to a given player's turn.</summary>
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

    /// <summary>Directly sets the consecutive-sixes counter, to test the "three sixes in a row forfeits the turn" edge case.</summary>
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
