using Microsoft.EntityFrameworkCore;
using Models;
using Services;

namespace PI_1ANO.Data;

// Roda uma vez, quando o sistema liga (chamado no Program.cs).
public static class DbInicial
{
    
    
public static async Task PrepararAsync(AppDbContext db)
{
    await db.Database.MigrateAsync();

    // Lista os usuários já cadastrados.
    var usuarios = await db.Usuarios
        .Select(u => new { u.Id, u.Email, u.Tipo })
        .ToListAsync();

    Console.WriteLine("\n--- USUÁRIOS CADASTRADOS ---");

    foreach (var usuario in usuarios)
    {
        Console.WriteLine(
            $"ID: {usuario.Id} | Email: [{usuario.Email}] | Tipo: [{usuario.Tipo}]"
        );
    }

    string email = "gestor@outlook.com";

    // Compara ignorando espaços e maiúsculas/minúsculas.
    bool existeUsuario = usuarios.Any(u =>
        string.Equals(
            u.Email?.Trim(),
            email,
            StringComparison.OrdinalIgnoreCase
        )
    );

    Console.WriteLine($"Gestor já existe? {existeUsuario}");

    if (!existeUsuario)
    {
        db.Usuarios.Add(new Usuario(
            nome: "Josué",
            email: email,
            senha: SenhaHasher.Gerar("gestor123"),
            tipo: "gestor"
        ));

        await db.SaveChangesAsync();
        Console.WriteLine("Gestor cadastrado!");
    }
}


}
