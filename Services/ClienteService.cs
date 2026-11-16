using Models;
using PI_1ANO.Data;
using Validators;


namespace Services;

public class ClienteService : UsuarioService
{
    public ClienteService(UsuarioValidator validator) : base(validator)
    {
    }
    private readonly UsuarioValidator _validator;

    
    // Cadastra o cliente
    public async Task<bool> CadastrarCliente(Usuario cliente)
    {
        if (!_validator.ValidarNome(cliente.Nome))
        {
            Console.WriteLine("Nome inválido");
            return false;
        }

        if (!_validator.ValidarEmail(cliente.Email))
        {
            Console.WriteLine("Email inválido");
            return false;
        }

        if (!_validator.ValidarSenha(cliente.Senha))
        {
            Console.WriteLine("Senha inválida");
            return false;
        }

        await using var db = new AppDbContext();
        db.Usuarios.Add(cliente);
        await db.SaveChangesAsync();

        return true;
    }
}