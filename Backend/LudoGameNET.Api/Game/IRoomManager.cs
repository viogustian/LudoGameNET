using System.Collections.Concurrent;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Models;

namespace LudoGameNET.Api.Game;

public interface IRoomManager
{
    RoomSession CreateRoom();
    RoomSession? GetRoom(string roomCode);
    bool JoinRoom(string roomCode, string connectionId);
    void LeaveRoom(string connectionId);
    bool TryClaimSeat(string roomCode, string connectionId, PlayerColor color);
    void StartGame(string roomCode);
    IEnumerable<RoomSession> GetAllRooms();
    void RemoveRoom(string roomCode);
}
