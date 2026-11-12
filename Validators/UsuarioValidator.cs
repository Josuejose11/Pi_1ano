using Models;
namespace Validators;

public class GestorValidator : Usuario
{
    public GestorValidator(string nome, string email, string senha) : base(nome, email, senha)
    {
    }
}