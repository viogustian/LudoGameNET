using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace LudoGameNET.Tests;

[TestFixture]
public class RoomManagerCacheTests
{
    private IMemoryCache _cache = null!;
    private RoomManager _roomManager = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new RoomCacheOptions
        {
            SlidingExpirationMinutes = 15,
            AbsoluteExpirationHours = 2
        });
        _roomManager = new RoomManager(_cache, options);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
    }

    [Test]
    public void CreateRoom_AddsRoomToCache_AndTracksActiveRoom()
    {
        // Act
        var room = _roomManager.CreateRoom();

        // Assert
        Assert.That(room, Is.Not.Null);
        Assert.That(room.RoomCode, Has.Length.EqualTo(6));

        var retrieved = _roomManager.GetRoom(room.RoomCode);
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.RoomCode, Is.EqualTo(room.RoomCode));

        var allRooms = _roomManager.GetAllRooms().ToList();
        Assert.That(allRooms, Has.Count.EqualTo(1));
        Assert.That(allRooms[0].RoomCode, Is.EqualTo(room.RoomCode));
    }

    [Test]
    public void GetRoom_CaseInsensitiveLookup()
    {
        // Arrange
        var room = _roomManager.CreateRoom();

        // Act
        var upper = _roomManager.GetRoom(room.RoomCode.ToUpperInvariant());
        var lower = _roomManager.GetRoom(room.RoomCode.ToLowerInvariant());

        // Assert
        Assert.That(upper, Is.Not.Null);
        Assert.That(lower, Is.Not.Null);
        Assert.That(upper!.RoomCode, Is.EqualTo(lower!.RoomCode));
    }

    [Test]
    public void GetRoom_NonExistent_ReturnsNull()
    {
        // Act
        var result = _roomManager.GetRoom("NONEXIST");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void RemoveRoom_RemovesFromCache_AndActiveKeys()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        Assert.That(_roomManager.GetAllRooms().Count(), Is.EqualTo(1));

        // Act
        _roomManager.RemoveRoom(room.RoomCode);

        // Assert
        Assert.That(_roomManager.GetRoom(room.RoomCode), Is.Null);
        Assert.That(_roomManager.GetAllRooms(), Is.Empty);
    }

    [Test]
    public void JoinRoom_AddsConnectionAndAssignsHost()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        const string conn1 = "conn-1";

        // Act
        var success = _roomManager.JoinRoom(room.RoomCode, conn1);

        // Assert
        Assert.That(success, Is.True);
        Assert.That(room.JoinOrder, Contains.Item(conn1));
        Assert.That(room.HostConnectionId, Is.EqualTo(conn1));
    }

    [Test]
    public void JoinRoom_MaxFourPlayers_RejectsFifth()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        _roomManager.JoinRoom(room.RoomCode, "c1");
        _roomManager.JoinRoom(room.RoomCode, "c2");
        _roomManager.JoinRoom(room.RoomCode, "c3");
        _roomManager.JoinRoom(room.RoomCode, "c4");

        // Act
        var fifthResult = _roomManager.JoinRoom(room.RoomCode, "c5");

        // Assert
        Assert.That(fifthResult, Is.False);
        Assert.That(room.JoinOrder, Has.Count.EqualTo(4));
    }

    [Test]
    public void TryClaimSeat_ClaimsSuccessfully_PreventsDuplicateClaim()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        _roomManager.JoinRoom(room.RoomCode, "c1");
        _roomManager.JoinRoom(room.RoomCode, "c2");

        // Act
        var claim1 = _roomManager.TryClaimSeat(room.RoomCode, "c1", PlayerColor.Red);
        var claim2 = _roomManager.TryClaimSeat(room.RoomCode, "c2", PlayerColor.Red); // Duplicate Red
        var claim3 = _roomManager.TryClaimSeat(room.RoomCode, "c2", PlayerColor.Blue);

        // Assert
        Assert.That(claim1, Is.True);
        Assert.That(claim2, Is.False);
        Assert.That(claim3, Is.True);
        Assert.That(room.PlayerConnections["c1"], Is.EqualTo(PlayerColor.Red));
        Assert.That(room.PlayerConnections["c2"], Is.EqualTo(PlayerColor.Blue));
    }

    [Test]
    public void StartGame_WithTwoPlayers_TransitionsToInGame()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        _roomManager.JoinRoom(room.RoomCode, "c1");
        _roomManager.JoinRoom(room.RoomCode, "c2");
        _roomManager.TryClaimSeat(room.RoomCode, "c1", PlayerColor.Red);
        _roomManager.TryClaimSeat(room.RoomCode, "c2", PlayerColor.Green);

        // Act
        _roomManager.StartGame(room.RoomCode);

        // Assert
        Assert.That(room.State, Is.EqualTo(RoomState.InGame));
        Assert.That(room.Game, Is.Not.Null);
        Assert.That(room.Game!.Players, Has.Count.EqualTo(2));
    }

    [Test]
    public void LeaveRoom_MigratesHost_AndReleasesSeat()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        _roomManager.JoinRoom(room.RoomCode, "c1");
        _roomManager.JoinRoom(room.RoomCode, "c2");
        _roomManager.TryClaimSeat(room.RoomCode, "c1", PlayerColor.Yellow);

        Assert.That(room.HostConnectionId, Is.EqualTo("c1"));

        // Act
        _roomManager.LeaveRoom("c1");

        // Assert
        Assert.That(room.JoinOrder, Does.Not.Contain("c1"));
        Assert.That(room.HostConnectionId, Is.EqualTo("c2"));
        Assert.That(room.PlayerConnections.ContainsKey("c1"), Is.False);
        Assert.That(room.ColorConnections.ContainsKey(PlayerColor.Yellow), Is.False);
    }

    [Test]
    public void EvictionCallback_WhenRoomRemoved_CleansUpConnectionMapping()
    {
        // Arrange
        var room = _roomManager.CreateRoom();
        _roomManager.JoinRoom(room.RoomCode, "conn-to-clean");
        _roomManager.TryClaimSeat(room.RoomCode, "conn-to-clean", PlayerColor.Red);

        // Act: Remove room, which triggers RegisterPostEvictionCallback
        _roomManager.RemoveRoom(room.RoomCode);

        // Assert: Leaving after eviction shouldn't throw or leave dirty state
        Assert.DoesNotThrow(() => _roomManager.LeaveRoom("conn-to-clean"));
        Assert.That(_roomManager.GetAllRooms(), Is.Empty);
    }

    [Test]
    public void OptionsPattern_CustomExpirationConfig_CreatesRoomSuccessfully()
    {
        // Arrange
        var customOptions = Options.Create(new RoomCacheOptions
        {
            SlidingExpirationMinutes = 30,
            AbsoluteExpirationHours = 5,
            SizeLimit = 500
        });
        var manager = new RoomManager(_cache, customOptions);

        // Act
        var room = manager.CreateRoom();

        // Assert
        Assert.That(room, Is.Not.Null);
        Assert.That(manager.GetRoom(room.RoomCode), Is.Not.Null);
    }
}
