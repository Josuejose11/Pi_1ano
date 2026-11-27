using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;

namespace Api;

public record NovoHistoricoDto(string? TipoEntidade, int EntidadeId, string? Acao, string? Descricao);

// Identifica o usuario 
public static class UsuarioAtual
{
    public static int? Id(HttpContext ctx)
        => int.TryParse(ctx.Request.Headers["X-Usuario-Id"].ToString(), out int id) ? id : null;

    // Procura o usuário no banco pelo cabeçalho. Devolve null se não mandou ou se não existe.
    public static async Task<Usuario?> Obter(HttpContext ctx, AppDbContext db)
    {
        int? id = Id(ctx);
        return id == null ? null : await db.Usuarios.FindAsync(id.Value);
    }

    public static bool EhGestor(Usuario u)
        => string.Equals(u.Tipo, "gestor", StringComparison.OrdinalIgnoreCase);
}

public static class HistoricoEndpoints
{
    // Registra ação no histórico
    public static Task Registrar(this HistoricoService service, HttpContext ctx,
        string tipoEntidade, int entidadeId, string acao, string? descricao, int? autorId = null)
        => service.RegistrarHistorico(autorId ?? UsuarioAtual.Id(ctx) ?? 0, tipoEntidade, entidadeId, acao, descricao);

    public static void MapHistoricoEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/historico");

        // read 
        rotas.MapGet("/", async (HttpContext ctx, AppDbContext db, HistoricoService service,
            int? usuarioId, string? nome, string? tipo, string? acao, DateTime? de, DateTime? ate, int? limite) =>
        {
            var ator = await UsuarioAtual.Obter(ctx, db);
            if (ator == null)
                return Resposta.Erro("Faça login para ver o histórico.", 401);

            bool gestor = UsuarioAtual.EhGestor(ator);

            var filtro = new HistoricoFiltro(
                UsuarioId: gestor ? usuarioId : ator.Id,   // cliente: sempre o próprio id
                Nome: gestor ? nome : null,
                TipoEntidade: tipo,
                Acao: acao,
                De: de,
                Ate: ate,
                Limite: limite ?? 200);

            return Results.Ok(await service.Listar(filtro));
        });

        //  BUSCAR POR NOME (só gestor) 
        rotas.MapGet("/buscar", async (string nome, HttpContext ctx, AppDbContext db, HistoricoService service) =>
        {
            var ator = await UsuarioAtual.Obter(ctx, db);
            if (ator == null)
                return Resposta.Erro("Faça login para ver o histórico.", 401);
            if (!UsuarioAtual.EhGestor(ator))
                return Resposta.Erro("Só o gestor pode buscar o histórico de outros usuários.", 403);

            return Results.Ok(await service.BuscarPorNome(nome));
        });

        //CREATE 
        rotas.MapPost("/", async (NovoHistoricoDto dto, HttpContext ctx, AppDbContext db, HistoricoService service) =>
        {
            var ator = await UsuarioAtual.Obter(ctx, db);
            if (ator == null)
                return Resposta.Erro("Faça login para registrar no histórico.", 401);
            if (!UsuarioAtual.EhGestor(ator))
                return Resposta.Erro("Só o gestor pode registrar manualmente no histórico.", 403);

            if (string.IsNullOrWhiteSpace(dto.TipoEntidade) || string.IsNullOrWhiteSpace(dto.Acao))
                return Resposta.Erro("Informe o tipo e a ação.");

            bool criado = await service.RegistrarHistorico(
                ator.Id, dto.TipoEntidade.Trim(), dto.EntidadeId, dto.Acao.Trim(), dto.Descricao?.Trim());

            if (!criado)
                return Resposta.Erro("Não foi possível gravar no histórico.", 500);

            return Results.Ok(new { mensagem = "Registro adicionado ao histórico." });
        });

    }
}

// Formata dinheiro como "R$ 24,90" para as descrições do histórico.
public static class TextoHistorico
{
    private static readonly CultureInfo Br = new("pt-BR");
    public static string Moeda(decimal? valor) => (valor ?? 0).ToString("C", Br);
}