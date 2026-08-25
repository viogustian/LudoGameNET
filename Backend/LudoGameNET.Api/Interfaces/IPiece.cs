using LudoGameNET.Api.Enums;

namespace LudoGameNET.Api.Interfaces;

public interface IPiece
{
    int Id { get; }
    PlayerColor Color { get; }
    PieceState State { get; set; }
    int? PathIndex { get; set; }
}