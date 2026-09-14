using System.Text.Json.Serialization;
using LudoGameNET.Api.Interfaces;

namespace LudoGameNET.Api.Models;

public class Board : IBoard
{
    [JsonIgnore]
    public Square[,] Squares { get; set; }

    public Board(Square[,] squares)
    {
        Squares = squares;
    }
}