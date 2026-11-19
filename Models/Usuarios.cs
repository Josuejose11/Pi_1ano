using Services;
namespace Models;
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public string Funcao { get; set; }

    public Usuario(string nome, string email, string senha, string funcao)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
        Funcao = funcao;
    }
}