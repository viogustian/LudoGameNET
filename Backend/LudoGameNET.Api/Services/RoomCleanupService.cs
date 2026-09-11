using LudoGameNET.Api.Game;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LudoGameNET.Api.Services;

public class RoomCleanupService : BackgroundService
{
    private readonly IRoomManager _roomManager;
    private readonly ILogger<RoomCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _staleThreshold = TimeSpan.FromMinutes(15);

    public RoomCleanupService(IRoomManager roomManager, ILogger<RoomCleanupService> logger)
    {
        _roomManager = roomManager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                CleanStaleRooms();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning stale rooms.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private void CleanStaleRooms()
    {
        var now = DateTime.UtcNow;
        var allRooms = _roomManager.GetAllRooms().ToList();
        
        int cleanedCount = 0;
        foreach (var room in allRooms)
        {
            if (now - room.LastActivity > _staleThreshold)
            {
                _roomManager.RemoveRoom(room.RoomCode);
                cleanedCount++;
                _logger.LogInformation("Removed stale room {RoomCode}", room.RoomCode);
            }
        }

        if (cleanedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} stale rooms.", cleanedCount);
        }
    }
}
