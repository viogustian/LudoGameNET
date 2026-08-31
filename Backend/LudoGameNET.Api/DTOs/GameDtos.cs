using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;

namespace LudoGameNET.Api.DTOs;

public class StartGameRequest
{
    public List<PlayerColor> Colors { get; set; } = new();
}

public class MovePieceRequest
{
    public int PieceId { get; set; }
    public int DiceValue { get; set; }
}

public class PieceDto
{
    public int Id { get; set; }
    public PlayerColor Color { get; set; }
    public PieceState State { get; set; }
    public int? PathIndex { get; set; }

    public static PieceDto From(IPiece piece) => new()
    {
        Id = piece.Id,
        Color = piece.Color,
        State = piece.State,
        PathIndex = piece.PathIndex,
    };
}

public class PlayerDto
{
    public int Id { get; set; }
    public PlayerColor Color { get; set; }
    public List<PieceDto> Pieces { get; set; } = new();
}

public class SquareDto
{
    public int Row { get; set; }
    public int Column { get; set; }
    public SquareType Type { get; set; }
    public PlayerColor? HomeColor { get; set; }
    public List<PieceDto> Pieces { get; set; } = new();
}

public class GameStateDto
{
    public GameState State { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int ConsecutiveSixes { get; set; }
    public int? LastDiceValue { get; set; }
    public List<PlayerDto> Players { get; set; } = new();
    public PlayerColor? WinnerColor { get; set; }
}

public class RollDiceResponseDto
{
    public int DiceValue { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public List<PieceDto> ValidPieces { get; set; } = new();
}

public class DevSetDiceRequest
{
    public int? Value { get; set; }
    public bool Lock { get; set; }
}

public class DevDiceStatusDto
{
    public int? ForcedValue { get; set; }
    public bool Locked { get; set; }
    public int? CurrentDiceValue { get; set; }
}

public class DevColorRequest
{
    public PlayerColor Color { get; set; }
}

public class DevForcePieceRequest
{
    public PlayerColor Color { get; set; }
    public int PieceId { get; set; }
    public PieceState State { get; set; }
    public int? PathIndex { get; set; }
}

public class DevSetTurnRequest
{
    public int PlayerIndex { get; set; }
}

public class DevSetSixesRequest
{
    public int Count { get; set; }
}
