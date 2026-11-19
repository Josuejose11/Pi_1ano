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
        Console.WriteLine($"Usuario de ID: {usuario.Id} | Nome: {usuario.Nome} adicionado com sucesso");
        return true;
    }

    // Lista todos os usuarios
    public static async Task<List<Usuario>> ListarUsuarios()
    {
        await using var db = new AppDbContext();
        return await db.Usuarios.ToListAsync();
    }

    // Update usuario 
    public async Task<bool> AtualizarUsuario(int id ,Usuario usuario)
    {
        if (!UsuarioValidator.ValidarNome(usuario.Nome, out string nomeTratado))
            return false;

        if (!UsuarioValidator.ValidarEmail(usuario.Email))
            return false;

        if (!UsuarioValidator.ValidarSenha(usuario.Senha))
            return false;

        await using var db = new AppDbContext();
        var usuario1 = await db.Usuarios.FindAsync(id);
        if (usuario1 == null)
        {
            Console.WriteLine("Usuario não encontrado");
            return false;
        }
        usuario1.Nome = usuario.Nome;
        usuario1.Email = usuario.Email;
        usuario1.Senha = usuario.Senha;
        usuario1.Tipo = usuario.Tipo;
        await db.SaveChangesAsync();
        Console.WriteLine($"Usuario de ID: {usuario1.Id} | Nome: {usuario1.Nome} foi atualizado com sucesso");
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