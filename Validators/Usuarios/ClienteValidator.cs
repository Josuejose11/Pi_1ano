using Models;


namespace Validators;

public class ClienteValidator : UsuarioValidator
{
    public bool ValidarCliente(Usuario cliente)
    {
        if (!ValidarNome(cliente.Nome))
        {
            Console.WriteLine("Nome inválido");
            return false;
        }

        if (!ValidarEmail(cliente.Email))
        {
            Console.WriteLine("Email inválido");
            return false;
        }

        if (!ValidarSenha(cliente.Senha))
        {
            Console.WriteLine("Senha inválida");
            return false;
        }

        return true;
    }

}