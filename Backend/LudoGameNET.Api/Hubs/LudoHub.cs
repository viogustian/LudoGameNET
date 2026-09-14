using System.Collections.Concurrent;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace LudoGameNET.Api.Hubs;

public class LudoHub : Hub
{
    private readonly IRoomManager _roomManager;
    private readonly ILogger<LudoHub> _logger;

    public LudoHub(IRoomManager roomManager, ILogger<LudoHub> logger)
    {
        _roomManager = roomManager;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        _roomManager.LeaveRoom(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<string> CreateRoom()
    {
        var room = _roomManager.CreateRoom();
        await JoinRoom(room.RoomCode);
        return room.RoomCode;
    }

    public async Task<bool> JoinRoom(string roomCode)
    {
        if (_roomManager.JoinRoom(roomCode, Context.ConnectionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            var room = _roomManager.GetRoom(roomCode);
            await Clients.Group(roomCode).SendAsync("RoomStateUpdated", room);
            return true;
        }
        return false;
    }

    public async Task<bool> TryClaimSeat(string roomCode, PlayerColor color)
    {
        if (_roomManager.TryClaimSeat(roomCode, Context.ConnectionId, color))
        {
            var room = _roomManager.GetRoom(roomCode);
            await Clients.Group(roomCode).SendAsync("RoomStateUpdated", room);
            return true;
        }
        return false;
    }

    public async Task StartGame(string roomCode)
    {
        _roomManager.StartGame(roomCode);
        var room = _roomManager.GetRoom(roomCode);
        if (room != null && room.State == RoomState.InGame)
        {
            await Clients.Group(roomCode).SendAsync("GameStarted", room);
        }
    }

    public async Task RollDice(string roomCode)
    {
        var room = _roomManager.GetRoom(roomCode);
        if (room == null || room.State != RoomState.InGame || room.Game == null) return;

        lock (room)
        {
            // Validate it's the caller's turn
            var currentPlayerColor = room.Game.GetCurrentPlayer().Color;
            if (!room.ColorConnections.TryGetValue(currentPlayerColor, out var connectionId) || connectionId != Context.ConnectionId)
                return;

            room.Game.RollDice();
            room.IncrementVersion();
        }

        await Clients.Group(roomCode).SendAsync("DiceRolled", room);
    }

    public async Task MovePiece(string roomCode, int pieceId, int diceValue)
    {
        var room = _roomManager.GetRoom(roomCode);
        if (room == null || room.State != RoomState.InGame || room.Game == null) return;

        lock (room)
        {
            // Validate it's the caller's turn
            var currentPlayer = room.Game.GetCurrentPlayer();
            if (!room.ColorConnections.TryGetValue(currentPlayer.Color, out var connectionId) || connectionId != Context.ConnectionId)
                return;

            var piece = currentPlayer.Pieces.FirstOrDefault(p => p.Id == pieceId);
            if (piece == null) return;

            room.Game.MovePiece(currentPlayer, piece, diceValue);
            room.IncrementVersion();
        }

        await Clients.Group(roomCode).SendAsync("PieceMoved", room);
    }

    public async Task PassTurn(string roomCode)
    {
        var room = _roomManager.GetRoom(roomCode);
        if (room == null || room.State != RoomState.InGame || room.Game == null) return;

        lock (room)
        {
            var currentPlayer = room.Game.GetCurrentPlayer();
            if (!room.ColorConnections.TryGetValue(currentPlayer.Color, out var connectionId) || connectionId != Context.ConnectionId)
                return;

            try
            {
                room.Game.PassTurn();
                room.IncrementVersion();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PassTurn failed");
                return;
            }
        }

        await Clients.Group(roomCode).SendAsync("RoomStateUpdated", room);
    }
}
