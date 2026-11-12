using Models;
namespace Validators;

public class UsuarioValidator : Usuario
{
    public UsuarioValidator(string nome, string email, string senha) : base(nome, email, senha)
    {
    }
}