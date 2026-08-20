using LudoGameNET.Api.Interfaces;

namespace LudoGameNET.Api.Models;

public class Board : IBoard
{
    public Square[,] Squares { get; set; }

    public Board(Square[,] squares)
    {
        Squares = squares;
    }
}