using LudoGameNET.Api.DTOs;
using LudoGameNET.Api.Enums;
using LudoGameNET.Api.Interfaces;
using LudoGameNET.Api.Models;

namespace LudoGameNET.Api.Mapping;
public static class GameStateMapper
{
    public static GameStateDto ToGameStateDto(LudoGame game)
    {
        IPlayer? winner = game.State == GameState.Finished
            ? game.Players.FirstOrDefault(game.CheckWinner)
            : null;

        return new GameStateDto
        {
            State = game.State,
            CurrentPlayerIndex = game.CurrentPlayerIndex,
            ConsecutiveSixes = game.ConsecutiveSixes,
            LastDiceValue = game.Dice.Value == 0 ? null : game.Dice.Value,
            Players = game.Players.Select(ToPlayerDto).ToList(),
            WinnerColor = winner?.Color,
        };
    }

    public static PlayerDto ToPlayerDto(IPlayer player) => new()
    {
        Id = player.Id,
        Color = player.Color,
        Pieces = player.Pieces.Select(PieceDto.From).ToList(),
    };
}
