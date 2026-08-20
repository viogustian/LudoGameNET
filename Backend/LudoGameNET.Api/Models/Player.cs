using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;

namespace LudoGameNET.Api.Models;

public class Player : IPlayer
{
    public int Id { get; set; }
    public PlayerColor Color { get; set; }
    public List<IPiece> Pieces { get; set; }

    public Player(int id, PlayerColor color, List<IPiece> pieces)
    {
        Id = id;
        Color = color;
        Pieces = pieces ?? new List<IPiece>();
    }
}