using Services;
namespace Models;
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Senha { get; set; }
    public string Tipo { get; set; } = "cliente"; // Valor padrão para novos usuários

    public Usuario(string nome, string email, string senha, string tipo="cliente")
    {
        Nome = nome;
        Email = email;
        Senha = senha;
        Tipo = tipo;
    }
}