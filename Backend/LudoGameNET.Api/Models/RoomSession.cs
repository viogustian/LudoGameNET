using System.Collections.Concurrent;
using LudoGameNET.Api.Enums;

namespace LudoGameNET.Api.Models;

public class RoomSession
{
    public string RoomCode { get; }
    public RoomState State { get; set; } = RoomState.Waiting;
    public LudoGame? Game { get; set; }
    public int Version { get; set; } = 0;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    // Map ConnectionId -> PlayerColor
    public ConcurrentDictionary<string, PlayerColor> PlayerConnections { get; } = new();

    // Map PlayerColor -> ConnectionId (for reverse lookup)
    public ConcurrentDictionary<PlayerColor, string> ColorConnections { get; } = new();

    // The current host of the room
    public string? HostConnectionId { get; set; }
    
    // Ordered list of connections by join order (for host migration)
    public List<string> JoinOrder { get; } = new();

    public RoomSession(string roomCode)
    {
        RoomCode = roomCode;
    }

    public void UpdateActivity()
    {
        LastActivity = DateTime.UtcNow;
    }

    public void IncrementVersion()
    {
        Version++;
        UpdateActivity();
    }
}
