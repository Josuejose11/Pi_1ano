using Models;


namespace Validators;

public class GestorValidator : UsuarioValidator
{
    public bool ValidarGestor(Gestor gestor)
    {
        if (!ValidarNome(gestor.Nome))
        {
            Console.WriteLine("Nome inválido");
            return false;
        }

        if (!ValidarEmail(gestor.Email))
        {
            Console.WriteLine("Email inválido");
            return false;
        }

        if (!ValidarSenha(gestor.Senha))
        {
            Console.WriteLine("Senha inválida");
            return false;
        }

        return true;
    }

}