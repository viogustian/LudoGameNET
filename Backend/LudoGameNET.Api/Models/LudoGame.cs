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
    public IBoard? Board { get; set; }
    public IDice Dice { get; set; }
    public Dictionary<PlayerColor, List<Point>>? Paths { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public int ConsecutiveSixes { get; set; }
    public GameState State { get; set; }

    public static List<Point> CommonPath = new List<Point>
    {
        new(6,1), new(6,2), new(6,3), new(6,4), new(6,5),
        new(5,6), new(4,6), new(3,6), new(2,6), new(1,6),
        new(0,6),
        new(0,7),
        new(0,8),
        new(1,8), new(2,8), new(3,8), new(4,8), new(5,8),
        new(6,9), new(6,10), new(6,11), new(6,12), new(6,13),
        new(6,14),
        new(7,14),
        new(8,14),
        new(8,13), new(8,12), new(8,11), new(8,10), new(8,9),
        new(9,8), new(10,8), new(11,8), new(12,8), new(13,8),
        new(14,8),
        new(14,7),
        new(14,6),
        new(13,6), new(12,6), new(11,6), new(10,6), new(9,6),
        new(8,5), new(8,4), new(8,3), new(8,2), new(8,1),
        new(8,0),
        new(7,0),
        new(6,0),
    };

    public static Dictionary<PlayerColor, int> StartOffsets = new Dictionary<PlayerColor, int>
    {
        [PlayerColor.Red] = 0,
        [PlayerColor.Green] = 13,
        [PlayerColor.Yellow] = 26,
        [PlayerColor.Blue] = 39,
    };

    public static HashSet<Point> SafeSquares = new HashSet<Point>(
        StartOffsets.Values.SelectMany(offset => new[]
    {
        CommonPath[offset],
        CommonPath[offset + 8],
    }));

    public static readonly Dictionary<PlayerColor, List<Point>> HomeStretches =
        new Dictionary<PlayerColor, List<Point>>
        {
                [PlayerColor.Red] = new List<Point> { new(7,1), new(7,2), new(7,3), new(7,4), new(7,5), new(7,6) },
                [PlayerColor.Green] = new List<Point> { new(1,7), new(2,7), new(3,7), new(4,7), new(5,7), new(6,7) },
                [PlayerColor.Yellow] = new List<Point> { new(7,13), new(7,12), new(7,11), new(7,10), new(7,9), new(7,8) },
                [PlayerColor.Blue] = new List<Point> { new(13,7), new(12,7), new(11,7), new(10,7), new(9,7), new(8,7) },
        };

    private static readonly Dictionary<PlayerColor, (int R0, int R1, int C0, int C1)> YardRegions =
        new Dictionary<PlayerColor, (int, int, int, int)>
        {
            [PlayerColor.Red] = (0, 5, 0, 5),
            [PlayerColor.Green] = (0, 5, 9, 14),
            [PlayerColor.Yellow] = (9, 14, 9, 14),
            [PlayerColor.Blue] = (9, 14, 0, 5),
        };

    public static readonly Dictionary<PlayerColor, List<Point>> YardHoldingPoints =
        new Dictionary<PlayerColor, List<Point>>
        {
            [PlayerColor.Red] = new List<Point> { new(1,1), new(1,4), new(4,1), new(4,4) },
            [PlayerColor.Green] = new List<Point> { new(1,10), new(1,13), new(4,10), new(4,13) },
            [PlayerColor.Yellow] = new List<Point> { new(10,10), new(10,13), new(13,10), new(13,13) },
            [PlayerColor.Blue] = new List<Point> { new(10,1), new(10,4), new(13,1), new(13,4) },
        };

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

        return (Board ?? throw new InvalidOperationException("The board has not been initialized.")).Squares[position.Row, position.Column];
    }

    public bool IsValidPosition(Point position) =>
        Board is not null && position.Row >= 0 && position.Row < Board.Squares.GetLength(0) &&
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
        var paths = Paths ?? throw new InvalidOperationException("The paths have not been initialized.");
        var point = paths[color][pathIndex];
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

    public List<Point> BuildPathForColor(PlayerColor color)
    {
        var offset = StartOffsets[color];
        var path = new List<Point>(TotalPathLength);

        for(int i = 0; i < CommonTrackLength - 1 ; i++)
        {
            path.Add(CommonPath[(offset + i) % CommonTrackLength]);
        }

        path.AddRange(HomeStretches[color]);
        return path;
    }

        public static Board CreateBoard()
    {
        var squares = new Square[BoardSize, BoardSize];
        var commonPathSet = new HashSet<Point>(CommonPath);

        var homeStretchLookup = new Dictionary<Point, PlayerColor>();
        var goalLookup = new Dictionary<Point, PlayerColor>();
        foreach (var (color, points) in HomeStretches)
        {
            for (var i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1)
                {
                    goalLookup[points[i]] = color; // last square of the stretch = that color's own Goal
                }
                else
                {
                    homeStretchLookup[points[i]] = color;
                }
            }
        }

        for (var row = 0; row < BoardSize; row++)
        {
            for (var col = 0; col < BoardSize; col++)
            {
                var position = new Point(row, col);
                squares[row, col] = BuildSquare(position, commonPathSet, homeStretchLookup, goalLookup);
            }
        }

        return new Board(squares);
    }

    public static Square BuildSquare(
        Point position,
        HashSet<Point> commonPathSet,
        Dictionary<Point, PlayerColor> homeStretchLookup,
        Dictionary<Point, PlayerColor> goalLookup)
    {
        if (goalLookup.TryGetValue(position, out var goalColor))
        {
            return new Square(position, SquareType.Goal, goalColor, new List<IPiece>());
        }

        if (homeStretchLookup.TryGetValue(position, out var stretchColor))
        {
            return new Square(position, SquareType.HomeStretch, stretchColor, new List<IPiece>());
        }

        if (commonPathSet.Contains(position))
        {
            var type = SafeSquares.Contains(position) ? SquareType.Safe : SquareType.Common;
            return new Square(position, type, default, new List<IPiece>());
        }

        var yardColor = YardRegions
            .Where(kv => position.Row >= kv.Value.R0 && position.Row <= kv.Value.R1
                      && position.Column >= kv.Value.C0 && position.Column <= kv.Value.C1)
            .Select(kv => (PlayerColor?)kv.Key)
            .FirstOrDefault();

        if (yardColor is not null)
        {
            return new Square(position, SquareType.Yard, yardColor.Value, new List<IPiece>());
        }

        return new Square(position, SquareType.Common, default, new List<IPiece>());
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
