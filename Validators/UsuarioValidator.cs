using Models;
namespace Validators;

public abstract class UsuarioValidator : Usuario
{
    public UsuarioValidator() : base("", "", "")
    {
    }

    public bool ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return false;
        }
        nome = string.Join(" ", nome.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Nome = char.ToUpper(nome[0]) + nome.Substring(1); 
        return true;
    }

    public bool ValidarEmail(string email)
    {
        Email = email.Replace(" ", "");
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        string[] arroba =
        {
            "@gmail.com",
            "@hotmail.com",
            "@outlook.com",
            "@yahoo.com"
        };

        if (!arroba.Any(a => Email.Contains(a)))
        {
            return false;
        }
        return true;
    }

    public bool ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha) || senha.Length < 6)
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

        Senha = senha;
        return true;
    }
}