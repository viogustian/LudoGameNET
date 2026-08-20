using LudoGameNET.Api.Enums;

namespace LudoGameNET.Api.Interfaces;

public interface IPlayer
{
    int Id { get; }
    PlayerColor Color { get; }
    List<IPiece> Pieces { get; }
}