using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;

namespace Api;

public record LoginDto(string? Email, string? Senha, string? Tipo);

public static class LoginEndpoints
{
    public static void MapLoginEndpoints(this IEndpointRouteBuilder app)
    {
        // POST /api/login
        app.MapPost("/api/login", async (LoginDto dto, AppDbContext db, HistoricoService historico, HttpContext ctx) =>
        {
            string email = (dto.Email ?? "").Replace(" ", "").ToLower();

            // Procura na tabela Usuarios (traz Cliente ou Gestor, conforme a coluna "Tipo").
            Usuario? usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            string? tipo = usuario?.Tipo;

            // Mensagem igual para "e-mail não existe" e "senha errada": não revela se o e-mail está cadastrado.
            if (usuario == null || !SenhaHasher.Conferir(dto.Senha ?? "", usuario.Senha) || !string.Equals(tipo, dto.Tipo, StringComparison.OrdinalIgnoreCase))
                return Resposta.Erro("E-mail ou senha incorretos.", 401);

            await historico.Registrar(ctx, EntidadeHistorico.Usuario, usuario.Id, AcaoHistorico.Entrou,
                $"Entrou no sistema como {tipo!.ToLower()}.", autorId: usuario.Id);

            // Devolve só o necessário. A senha (hash) nunca sai do servidor.
            return Results.Ok(new { usuario.Id, usuario.Nome, usuario.Email, tipo = tipo!.ToLower()});
        });
    }
}
