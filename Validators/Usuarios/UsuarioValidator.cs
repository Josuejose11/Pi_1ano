using Models;
using PI_1ANO.Data;
namespace Validators;

public abstract class UsuarioValidator
{

    public bool ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome.Trim()))                            
        {
            return false;
        }
        nome = string.Join(" ", nome.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return true;
    }

    public bool ValidarEmail(string email)
    {
        // Lista de arrobas para validação de email
        string[] arroba =
        {
            "@gmail.com",
            "@hotmail.com",
            "@outlook.com",
            "@yahoo.com"
        };


        string Email = email.Replace(" ", "");
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (!arroba.Any(a => Email.Contains(a)))
        {
            return false;
        }
        return true;
    }

    public bool ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha.Trim()) || senha.Length < 6)
        {
            return false;
        }

        bool letra = senha.Any(c => char.IsLetter(c));
        bool num = senha.Any(c => char.IsDigit(c));
        bool carac = senha.Any(c => !char.IsLetterOrDigit(c));
 
        if (senha.Length > 10 || senha.Length < 1)
        {
            return false;    
        }
           
           
        if (!letra && !num && !carac)
        {      
            return false;
        }

        return true;
    }
}