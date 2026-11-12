using Services;
namespace Models;

public class Cliente : Usuario
{
    public Cliente(string nome, string email, string senha) : base(nome, email, senha)
    {
    }
}