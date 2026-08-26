using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using LudoGameNET.Api.Enums;
using System.Linq;

namespace LudoGameNET.Api.Models;

public class LudoGame
{
    private const int MinPlayers = 2;
    private const int MaxPlayers = 4;
    private const int PiecesPerPlayer = 4;
    private const int MaxConsecutiveSixes = 3;
    public List<IPlayer> Players { get; set; }
    public IBoard Board { get; set; }
    public IDice Dice { get; set; }
    public Dictionary<PlayerColor, List<Point>> Paths { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int ConsecutiveSixes { get; set; }
    public GameState State { get; set; }

    public LudoGame(List<PlayerColor> playerColors, IDice? dice = null)
    {
        List<PlayerColor> colors = playerColors ?? throw new ArgumentNullException(nameof(playerColors));

        if(colors.Count < MinPlayers || colors.Count > MaxPlayers)
        {
            throw new ArgumentException($"Player must be Min {MinPlayers} or Max {MaxPlayers} players! ");

        }
        
        if(colors.Distinct().Count() != colors.Count)
        {
            throw new ArgumentException($"Each player must have distinct color.");
        }

        Dice = dice?? new Dice();
        
        CreateBoard();
        CreatePaths();

        Players = colors
            .Select((color, index) => (IPlayer) new Player(index, color, CreatePiecesForColor(color)))
            .ToList();

        CurrentPlayerIndex = 0;
        ConsecutiveSixes = 0;
        State = GameState.NotStarted;

    }

    public void StartGame()
    {
        State = GameState.Playing;
    }

    public static List<IPiece> CreatePiecesForColor(PlayerColor color) =>
        Enumerable.Range(0,PiecesPerPlayer).Select(id => (IPiece) new Piece(id, color, PieceState.Base, null))
        .ToList();   

    public void CreatePaths()
    {
        Paths = Enum.GetValues<PlayerColor>().ToDictionary(color => color, BuildPathForColor);
    }

    public IPlayer GetCurrentPlayer() => Players[CurrentPlayerIndex];

    public int RollDice()
    {
        var value = Random.Shared.Next(1, 7);
        Dice.Value = value;
        return value;
    }

    public List<IPiece> GetValidPieces(IPlayer player, int diceValue)
    {
        var  validPieces = new List<IPiece>();

        foreach(var piece in player.Pieces)
        {
            switch(piece.State)
            {
                case PieceState.Base when CanEnterBoard(piece, diceValue):
                    validPieces.Add(piece);
                    break;
                case PieceState.OnBoard when CanMove(piece, diceValue):
                    validPieces.Add(piece);
                    break;
            }
        }

        return validPieces;
    }

    public Square GetSquare(Point position)
    {
        if(!IsValidPosition(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position is outside the board!");
        }

        return Board.Squares[position.Row, position.Column];
    }

    public bool IsValidPosition(Point position) =>
        position.Row >=0 && position.Row <= Board.Squares.GetLength(0) &&
        position.Column >=0 && position.Column <= Board.Squares.GetLength(1);


}