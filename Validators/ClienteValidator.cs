using Models;


namespace Validators;

public class ClienteValidator : UsuarioValidator
{
    public ClienteValidator(string nome, string email, string senha) : base(nome, email, senha)
    {
    }

}