using System.Collections.Concurrent;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace LudoGameNET.Api.Game;

public class RoomManager : IRoomManager
{
    private readonly ConcurrentDictionary<string, RoomSession> _rooms = new();
    private readonly ConcurrentDictionary<string, string> _connectionToRoom = new();
    private readonly ILogger<RoomManager> _logger;
    private readonly ILogger<LudoGame> _gameLogger;

    public RoomManager(ILogger<RoomManager>? logger = null, ILogger<LudoGame>? gameLogger = null)
    {
        _logger = logger ?? NullLogger<RoomManager>.Instance;
        _gameLogger = gameLogger ?? NullLogger<LudoGame>.Instance;
    }

    public RoomSession CreateRoom()
    {
        string code = GenerateRoomCode();
        while (_rooms.ContainsKey(code))
        {
            code = GenerateRoomCode();
        }

        var room = new RoomSession(code);
        _rooms.TryAdd(code, room);
        _logger.LogInformation("Room {RoomCode} created", code);
        return room;
    }

    public RoomSession? GetRoom(string roomCode)
    {
        _rooms.TryGetValue(roomCode.ToUpperInvariant(), out var room);
        return room;
    }

    public bool JoinRoom(string roomCode, string connectionId)
    {
        var room = GetRoom(roomCode);
        if (room == null || room.State != RoomState.Waiting)
            return false;

        lock (room)
        {
            if (room.JoinOrder.Contains(connectionId))
                return true; // Already joined

            if (room.JoinOrder.Count >= 4)
                return false; // Full

            room.JoinOrder.Add(connectionId);
            _connectionToRoom[connectionId] = room.RoomCode;

            if (room.HostConnectionId == null)
            {
                room.HostConnectionId = connectionId;
            }

            room.UpdateActivity();
            return true;
        }
    }

    public void LeaveRoom(string connectionId)
    {
        if (_connectionToRoom.TryRemove(connectionId, out var roomCode))
        {
            var room = GetRoom(roomCode);
            if (room != null)
            {
                lock (room)
                {
                    room.JoinOrder.Remove(connectionId);
                    
                    // Remove from seats
                    foreach (var kvp in room.PlayerConnections)
                    {
                        if (kvp.Key == connectionId)
                        {
                            room.PlayerConnections.TryRemove(kvp.Key, out _);
                            room.ColorConnections.TryRemove(kvp.Value, out _);
                            break;
                        }
                    }

                    if (room.HostConnectionId == connectionId)
                    {
                        room.HostConnectionId = room.JoinOrder.FirstOrDefault();
                    }

                    room.UpdateActivity();
                }
            }
        }
    }

    public bool TryClaimSeat(string roomCode, string connectionId, PlayerColor color)
    {
        var room = GetRoom(roomCode);
        if (room == null || room.State != RoomState.Waiting) return false;

        lock (room)
        {
            if (!room.JoinOrder.Contains(connectionId)) return false;

            // Check if already claimed by someone else
            if (room.ColorConnections.TryGetValue(color, out var existingConnectionId))
            {
                if (existingConnectionId != connectionId)
                    return false;
            }

            // Remove previous claim by this connection
            foreach (var kvp in room.PlayerConnections)
            {
                if (kvp.Key == connectionId)
                {
                    room.PlayerConnections.TryRemove(kvp.Key, out _);
                    room.ColorConnections.TryRemove(kvp.Value, out _);
                    break;
                }
            }

            room.PlayerConnections[connectionId] = color;
            room.ColorConnections[color] = connectionId;
        }
        return true;
    }

    public void StartGame(string roomCode)
    {
        var room = GetRoom(roomCode);
        if (room == null || room.State != RoomState.Waiting) return;

        lock (room)
        {
            if (room.ColorConnections.Count < 2) return;

            var colors = room.ColorConnections.Keys.ToList();
            room.Game = new LudoGame(colors, logger: _gameLogger);
            room.Game.StartGame();
            room.State = RoomState.InGame;
            room.IncrementVersion();
        }
    }

    public IEnumerable<RoomSession> GetAllRooms() => _rooms.Values;

    public void RemoveRoom(string roomCode)
    {
        _rooms.TryRemove(roomCode, out _);
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
