using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;

namespace Api;

// O que o HTML manda no corpo da requisição (JSON): {"email": "...", "senha": "...", "tipo": "cliente"}
// O ASP.NET converte o JSON nesse "record" automaticamente.
public record LoginDto(string? Email, string? Senha, string? Tipo);

public static class LoginEndpoints
{
    // "this IEndpointRouteBuilder app" faz deste método uma extensão: dá para chamar
    // app.MapLoginEndpoints() no Program.cs como se fosse um método do próprio "app".
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /api/login
        // Os parâmetros da lambda são preenchidos pelo ASP.NET:
        //   dto -> vem do corpo JSON;  db -> vem do container de serviços (Program.cs).
        app.MapPost("/api/login", async (LoginDto dto, AppDbContext db) =>
        {
            string email = (dto.Email ?? "").Replace(" ", "").ToLower();

            // Procura na tabela Usuarios (traz Cliente ou Gestor, conforme a coluna "Tipo").
            Usuario? usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            string? tipo = usuario?.Tipo;

            // Mensagem igual para "e-mail não existe" e "senha errada": não revela se o e-mail está cadastrado.
            if (usuario == null || !SenhaHasher.Conferir(dto.Senha ?? "", usuario.Senha) || !string.Equals(tipo, dto.Tipo, StringComparison.OrdinalIgnoreCase))
                return Resposta.Erro("E-mail ou senha incorretos.", 401);

            // Devolve só o necessário. A senha (hash) nunca sai do servidor.
            return Results.Ok(new { usuario.Id, usuario.Nome, usuario.Email, tipo });
        });
    }
}
