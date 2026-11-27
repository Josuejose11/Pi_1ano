using Models;
using PI_1ANO.Data;
namespace Validators;

public static class UsuarioValidator
{
    // VALIDACAO TOTAL DO USUARIO
    public static bool ValidarUsuario(Usuario usuario)
    {
        if (!ValidarNome(usuario.Nome, out string nomeTratado))
        {
            Console.WriteLine("Nome inválido");
            return false;
        }

        if (!ValidarEmail(usuario.Email))
        {
            Console.WriteLine("Email inválido");
            return false;
        }

        if (!ValidarSenha(usuario.Senha))
        {
            Console.WriteLine("Senha inválida");
            return false;
        }

        if (!ValidarFuncao(usuario.Tipo))
        {
            Console.WriteLine("Função inválida");
            return false;
        }

        return true;
    }

    // Validacao nome
    public static bool ValidarNome(string nome, out string nomeTratado)
    {
        nomeTratado = string.Empty;
        if (!nome.Any(char.IsLetter))                            
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(nome.Trim()))                            
        {
            return false;
        }
        nomeTratado = string.Join(" ", nome.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        nomeTratado = char.ToUpper(nomeTratado[0]) + nomeTratado[1..];
        return true;
    }

    // Validacao email
    public static bool ValidarEmail(string email)
    {
        // Lista de arrobas para validação de email
        string[] arroba =
        {
            "@gmail.com",
            "@hotmail.com",
            "@outlook.com",
            "@yahoo.com"
        };


        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        if (!arroba.Any(a => email.Contains(a)))
        {
            return false;
        }
        return true;
    }

    // Validacao senha
    public static bool ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha.Trim()) || senha.Length < 6)
        {
            return false;
        }

        if (senha.Length > 10 || senha.Length < 6)
        {
            return false;    
        }
           
        return true;

    }

    // Validacao funcao
    public static bool ValidarFuncao(string funcao)
    {
        if (string.IsNullOrWhiteSpace(funcao.Trim()))
        {
            return false;
        }


        if (funcao != "Administrador" && funcao != "Cliente")
        {
            return false;
        }
        return true;
    }

}