using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;
using LudoGameNET.Api.Enums;
using System.Linq;
using System.Text.RegularExpressions;

namespace LudoGameNET.Api.Models;

public class LudoGame
{
    private const int MinPlayers = 2;
    private const int MaxPlayers = 4;
    private const int PiecesPerPlayer = 4;
    private const int MaxConsecutiveSixes = 3;
    public const int BoardSize = 15;
    public const int CommonTrackLength = 52;
    public const int HomeStretchLength = 6;
    public const int TotalPathLength = CommonTrackLength - 1 + HomeStretchLength;
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
        position.Row >=0 && position.Row < Board.Squares.GetLength(0) &&
        position.Column >=0 && position.Column < Board.Squares.GetLength(1);

    public bool IsSafePosition(Square square) => 

        square.Type == SquareType.Safe ||
        square.Type == SquareType.Yard ||
        square.Type == SquareType.HomeStretch ||
        square.Type == SquareType.Goal;

    public bool CanEnterBoard(IPiece piece, int diceValue) =>
        piece.State == PieceState.Base && diceValue == 6;
    
    public bool CanMove(IPiece piece, int diceValue)
    {
        if(piece.State != PieceState.OnBoard)
        {
            return false;
        }

        var nextIndex = GetNextPathIndex(piece, diceValue);
        return nextIndex <= TotalPathLength -1;
    }

    public bool HasReachedFinish(IPiece piece) =>
        piece.State == PieceState.Finished || piece.PathIndex == TotalPathLength - 1;
    
    public int GetNextPathIndex(IPiece piece, int steps) =>
        piece.PathIndex.GetValueOrDefault() + steps;

    public Square GetSquareAtPathIndex(PlayerColor color, int pathIndex)
    {
        var point = Paths[color][pathIndex];
        return GetSquare(point);
    }

    public void CapturePiece(IPiece piece, Square square)
    {
        square.Pieces.Remove(piece);
        piece.State = PieceState.Base;
        piece.PathIndex = null;
    }
    
    public void HandleCapture(IPiece piece, Square square)
    {

        if(IsSafePosition(square))
        {
            return;
        }

        var opponentPieces = new List<IPiece>();

        foreach(var p in square.Pieces)
        {
            if(p.Color != piece.Color)
            {
                opponentPieces.Add(p);
            }
        }

        foreach (var opponentPiece in opponentPieces)
        {
            CapturePiece(opponentPiece, square);
        }
    }

    public bool CheckWinner(IPlayer player) =>
        player.Pieces.All(p => p.State == PieceState.Finished);

    public void EndGame()
    {
        State = GameState.Finished;
    }

    public void NextTurn()
    {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
    }

    public void HandleTurnAfterMove(int diceValue)
    {
        if(diceValue == 6)
        {
            ConsecutiveSixes++;

            if(ConsecutiveSixes >= MaxConsecutiveSixes)
            {
                ConsecutiveSixes = 0;
                NextTurn();
            }

            return;
        }

        ConsecutiveSixes = 0;
        NextTurn();
    }

    public void MovePiece(IPlayer player, IPiece piece, int diceValue)
    {
        if (State != GameState.Playing)
        {
            throw new InvalidOperationException("The game is not currently in progress.");
        }

        if (!player.Pieces.Contains(piece))
        {
            throw new ArgumentException("The piece does not belong to the given player.");
        }
        
        if(piece.State == PieceState.Base)
        {
            if(!CanEnterBoard(piece, diceValue))
            {
                throw new InvalidOperationException("");
            }
            
            piece.State = PieceState.OnBoard;
            piece.PathIndex = 0;

            var startSquare = GetSquareAtPathIndex(player.Color, piece.PathIndex.GetValueOrDefault());
            startSquare.Pieces.Add(piece);
            HandleCapture(piece, startSquare);
        }
        else
        {
            if(!CanMove(piece, diceValue))
            {
                throw new InvalidOperationException("The piece cannot move that many steps.");
            }

            var oldSquare = GetSquareAtPathIndex(player.Color, piece.PathIndex.GetValueOrDefault());
            oldSquare.Pieces.Remove(piece);

            var newIndex = GetNextPathIndex(piece, diceValue);
            piece.PathIndex = newIndex;

            if(HasReachedFinish(piece))
            {
                piece.State = PieceState.Finished;
            }
            else
            {
                var newSquare = GetSquareAtPathIndex(player.Color, newIndex);
                newSquare.Pieces.Add(piece);
                HandleCapture(piece, newSquare);
            }
        }

        if(CheckWinner(player))
        {
            EndGame();
            return;
        }

        HandleTurnAfterMove(diceValue);
    }

    

}