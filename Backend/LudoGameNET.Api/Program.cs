using LudoGameNET.Api.Models;
using LudoGameNET.Api.Game;
using LudoGameNET.Api.Hubs;
using LudoGameNET.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

try
{
    Log.Information("Starting up the application...");

    builder.Host.UseSerilog();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddSignalR()
        .AddJsonProtocol(options => 
        {
            options.PayloadSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Ludo Game API",
            Version = "v1",
            Description = "Web API backend for a Ludo (Parcheesi-style) board game, with SignalR multiplayer."
        });
    });

    builder.Services.AddSingleton<IRoomManager, RoomManager>();
    builder.Services.AddHostedService<RoomCleanupService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials());
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<LudoHub>("/hubs/ludo");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}