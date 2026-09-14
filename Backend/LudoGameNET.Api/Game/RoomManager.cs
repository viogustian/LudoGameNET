using System.Collections.Concurrent;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LudoGameNET.Api.Game;

public class RoomManager : IRoomManager
{
    private readonly IMemoryCache _cache;
    private readonly RoomCacheOptions _options;
    private readonly ConcurrentDictionary<string, byte> _activeRoomCodes = new();
    private readonly ConcurrentDictionary<string, string> _connectionToRoom = new();
    private readonly ILogger<RoomManager> _logger;
    private readonly ILogger<LudoGame> _gameLogger;

    public RoomManager(
        IMemoryCache cache,
        IOptions<RoomCacheOptions>? options = null,
        ILogger<RoomManager>? logger = null,
        ILogger<LudoGame>? gameLogger = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _options = options?.Value ?? new RoomCacheOptions();
        _logger = logger ?? NullLogger<RoomManager>.Instance;
        _gameLogger = gameLogger ?? NullLogger<LudoGame>.Instance;
    }

    public RoomSession CreateRoom()
    {
        string code = GenerateRoomCode();
        while (_activeRoomCodes.ContainsKey(code) || _cache.TryGetValue(code, out _))
        {
            code = GenerateRoomCode();
        }

        var room = new RoomSession(code);
        _activeRoomCodes.TryAdd(code, 0);

        var cacheEntryOptions = CreateCacheEntryOptions(code);
        _cache.Set(code, room, cacheEntryOptions);

        _logger.LogInformation("Room {RoomCode} created and cached in-memory (Sliding: {Sliding}m, Absolute: {Absolute}h)",
            code, _options.SlidingExpirationMinutes, _options.AbsoluteExpirationHours);

        return room;
    }

    public RoomSession? GetRoom(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return null;

        string normalizedCode = roomCode.ToUpperInvariant();
        if (_cache.TryGetValue(normalizedCode, out RoomSession? room))
        {
            return room;
        }

        return null;
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
            room.UpdateActivity();
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

    public IEnumerable<RoomSession> GetAllRooms()
    {
        var rooms = new List<RoomSession>();
        foreach (var code in _activeRoomCodes.Keys)
        {
            if (_cache.TryGetValue(code, out RoomSession? room) && room != null)
            {
                rooms.Add(room);
            }
            else
            {
                _activeRoomCodes.TryRemove(code, out _);
            }
        }
        return rooms;
    }

    public void RemoveRoom(string roomCode)
    {
        if (string.IsNullOrWhiteSpace(roomCode)) return;

        string normalizedCode = roomCode.ToUpperInvariant();
        _activeRoomCodes.TryRemove(normalizedCode, out _);
        _cache.Remove(normalizedCode);
    }

    private MemoryCacheEntryOptions CreateCacheEntryOptions(string roomCode)
    {
        var options = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(_options.SlidingExpirationMinutes),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_options.AbsoluteExpirationHours)
        };

        if (_options.SizeLimit.HasValue)
        {
            options.SetSize(1);
        }

        options.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            OnRoomEvicted(key?.ToString() ?? roomCode, value as RoomSession, reason);
        });

        return options;
    }

    private void OnRoomEvicted(string roomCode, RoomSession? room, EvictionReason reason)
    {
        _activeRoomCodes.TryRemove(roomCode, out _);

        // Clean up connection mappings for this room
        if (room != null)
        {
            foreach (var connId in room.PlayerConnections.Keys)
            {
                _connectionToRoom.TryRemove(connId, out _);
            }
            foreach (var connId in room.JoinOrder)
            {
                _connectionToRoom.TryRemove(connId, out _);
            }
        }
        else
        {
            foreach (var kvp in _connectionToRoom.ToArray())
            {
                if (kvp.Value == roomCode)
                {
                    _connectionToRoom.TryRemove(kvp.Key, out _);
                }
            }
        }

        _logger.LogInformation("Room {RoomCode} evicted from cache. Reason: {Reason}", roomCode, reason);
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
