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

    app.UseForwardedHeaders(new Microsoft.AspNetCore.Builder.ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                           Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });

    // Enable Swagger for interactive API exploration & verification in both dev and production
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ludo Game API v1");
        c.RoutePrefix = "swagger";
    });

    app.UseCors("AllowAll");

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<LudoHub>("/hubs/ludo");

    // Health check and root info endpoints for cloud hosting monitoring (Render, Koyeb, etc.)
    app.MapGet("/", () => Results.Ok(new
    {
        name = "LudoGameNET API",
        status = "healthy",
        version = "1.0.0",
        signalrHub = "/hubs/ludo",
        swagger = "/swagger",
        serverTime = DateTime.UtcNow
    }));

    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        serverTime = DateTime.UtcNow
    }));

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