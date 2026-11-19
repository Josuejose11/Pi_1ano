using Models;
using PI_1ANO.Data;
using Validators;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class UsuarioService
{
    
    // Cadastra o usuario 
    public async Task<bool> CadastrarUsuario(Usuario usuario)
    {
        if (!UsuarioValidator.ValidarUsuario(usuario))
            return false;

        await using (var db = new AppDbContext())
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        return true;
    }

    // Lista todos os usuarios
    public static async Task<List<Usuario>> ListarUsuarios()
    {
        await using var db = new AppDbContext();
        return await db.Usuarios.ToListAsync();
    }

    // Update usuario 
    public async Task<bool> AtualizarUsuario(Usuario usuario)
    {
        if (!UsuarioValidator.ValidarNome(usuario.Nome))
            return false;

        if (!UsuarioValidator.ValidarEmail(usuario.Email))
            return false;

        if (!UsuarioValidator.ValidarSenha(usuario.Senha))
            return false;

        await using var db = new AppDbContext();
        db.Usuarios.Update(usuario);
        await db.SaveChangesAsync();

        return true;
    }

    // Delete usuario
    public async Task<bool> DeletarUsuario(int id)
    {
        await using var db = new AppDbContext();
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario == null)
            return false;

        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();

        return true;
    }
    
    
}