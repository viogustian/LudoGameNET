using System.Text.Json.Serialization;
using LudoGameNET.Api.Models;
namespace LudoGameNET.Api.Interfaces;

public interface IBoard
{
    [JsonIgnore]
    Square[,] Squares {get;}
}