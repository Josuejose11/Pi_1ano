using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace PI_1ANO.Data;

// Roda uma vez, quando o sistema liga (chamado no Program.cs).
public static class DbInicial
{
    
    public static async Task PrepararAsync(AppDbContext db)
    {
        // Aplica as migrations pendentes no banco de dados.
        await db.Database.MigrateAsync();

        // Verifica se já existe um usuário do tipo gestor.
        bool existeGestor = await db.Usuarios
            .AnyAsync(u => u.Tipo == "gestor");

        // Se não existir, cria o gestor inicial.
        if (!existeGestor)
        {
            db.Usuarios.Add(new Usuario(
                nome: "Josué",
                tipo: "Gestor",
                email: "gestor@outlook.com",
                senha: SenhaHasher.Gerar("gestor123")
            ));

            await db.SaveChangesAsync();
        }
    }

}
