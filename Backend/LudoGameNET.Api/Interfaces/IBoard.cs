using LudoGameNET.Api.Models;
namespace LudoGameNET.Api.Interfaces;

public interface IBoard
{
    Square[,] Squares {get;}
}