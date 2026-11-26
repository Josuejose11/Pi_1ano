
using Models;
using Services;

namespace Api;

public record HistoricoDto(int UsuarioId, string TipoEntidade, int EntidadeId, string Acao, string? Descricao, DateTime DataHora);

public static class HistoricoEndpoints
{
    public static void MapHistoricoEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/Historico");

        // ---------- READ ----------
        rotas.MapGet("/", async (HistoricoService service) =>
        {
            var lista = await service.VerHistorico();

            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- BUSCAR POR NOME ----------
        rotas.MapGet("/buscar", async (string nome, HistoricoService service) =>
        {
            var lista = await service.BuscarPorNome(nome);

            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (HistoricoDto dto, HistoricoService service) =>
        {
            var historico = new Historico
            (
                dto.UsuarioId,
                dto.TipoEntidade,
                dto.EntidadeId,
                dto.Acao,
                dto.Descricao
            );

            bool criado = await service.RegistrarHistorico(historico.UsuarioId, historico.TipoEntidade, historico.EntidadeId, historico.Acao, historico.Descricao);

            if (!criado)
                return Resposta.Erro("Não foi possível cadastrar o produto. Verifique os dados.");

            return Results.Ok(Mapear(historico));
        });
    }

        

    // ---------- MAPEAR ----------
    private static object Mapear(Historico h) => new
    {
        h.UsuarioId,
        h.TipoEntidade, 
        h.EntidadeId, 
        h.Acao,
        h.Descricao, 
        h.DataHora
    };

}
