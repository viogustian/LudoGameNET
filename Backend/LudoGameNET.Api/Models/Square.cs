using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;

namespace LudoGameNET.Api.Models;

public class Square
{
    public Point Position { get; set; }
    public SquareType Type { get; set; }
    public PlayerColor? HomeColor { get; set; }
    public List<IPiece> Pieces { get; set; }

    public Square(Point position, SquareType type, PlayerColor homeColor, List<IPiece> pieces)
    {
        Position = position;
        Type = type;
        HomeColor = homeColor;
        Pieces = pieces ?? new List<IPiece>();
    }
}
