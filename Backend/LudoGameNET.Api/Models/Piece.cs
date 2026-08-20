using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
namespace LudoGameNET.Api.Models;

public class Piece : IPiece
{
    public int Id { get; }
    public PlayerColor Color { get; }
    public PieceState State { get; set; }
    public int PathIndex { get; set; }

    public Piece (int id, PlayerColor color, PieceState state, int pathIndex)
    {
        Id = id;
        Color = color;
        State = state;
        PathIndex = pathIndex;
    }
}