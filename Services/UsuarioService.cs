using Models;
using PI_1ANO.Data;
using Validators;
using Microsoft.EntityFrameworkCore;
namespace Services;

public class UsuarioService
{
    private readonly UsuarioValidator _validator;

    public UsuarioService(UsuarioValidator validator)
    {
        _validator = validator;
    }
    
    // Cadastra o usuario 
    public async Task<bool> CadastrarUsuario(Usuario usuario)
    {
        if (!_validator.ValidarNome(usuario.Nome))
            return false;

        if (!_validator.ValidarEmail(usuario.Email))
            return false;

        if (!_validator.ValidarSenha(usuario.Senha))
            return false;

        await using (var db = new AppDbContext())
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        return true;
    }

    // Lista todos os usuarios
    public async Task<List<Usuario>> ListarUsuarios()
    {
        await using var db = new AppDbContext();
        return await db.Usuarios.ToListAsync();
    }

    // Update usuario 
    
}