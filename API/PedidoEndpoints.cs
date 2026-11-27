
using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;

namespace Api;

public record PedidoItemDto(int ProdutoId, int Quantidade);
public record NovoPedidoDto(int ClienteId, List<PedidoItemDto>? Itens);
public record StatusDto(string? Status);

public static class PedidoEndpoints
{
    public static void MapPedidoEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/pedidos");

        // ---------- READ ----------
        rotas.MapGet("/", async (int? clienteId) =>
{
        var lista = await PedidoService.ListarPedidos();
        if (clienteId.HasValue)
            lista = lista.Where(p => p.ClienteId == clienteId.Value).ToList();

        return Results.Ok(lista.Select(Mapear));
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (NovoPedidoDto dto, HistoricoService historico, HttpContext ctx, AppDbContext db) =>
        {
            if (dto.Itens == null || dto.Itens.Count == 0)
                return Resposta.Erro("O carrinho está vazio.");

            var itens = dto.Itens.Select(i => new ItemPedido
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade
            }).ToList();

            var pedido = await PedidoService.CreatePedido(dto.ClienteId, itens);

            if (pedido == null)
                return Resposta.Erro("Não foi possível criar o pedido.");

            var cliente = await db.Usuarios.FindAsync(dto.ClienteId);
            decimal total = pedido.Itens.Sum(i => (i.PrecoUnitario ?? 0) * i.Quantidade);
            await historico.Registrar(ctx, EntidadeHistorico.Pedido, pedido.Id, AcaoHistorico.Criou,
                $"Pedido #{pedido.Id} de {cliente?.Nome} criado com {pedido.Itens.Count} item(ns), total {TextoHistorico.Moeda(total)}.");

            return Results.Ok(Mapear(pedido));
        });

        
        // UPDATE (status)
        rotas.MapPut("/{id:int}/status", async (int id, StatusDto dto, HistoricoService historico, HttpContext ctx) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Status))
                return Resposta.Erro("Status inválido.");

            var atualizado = await PedidoService.AtualizarStatus(
                id,
                dto.Status.Trim()
            );

            if (!atualizado)
                return Resposta.Erro("Não foi possível atualizar o pedido.");

            // O serviço só aceita "Concluído" ou "Cancelado"; cada um vira uma ação do histórico.
            bool concluiu = dto.Status.Trim() == "Concluído";
            await historico.Registrar(ctx, EntidadeHistorico.Pedido, id,
                concluiu ? AcaoHistorico.Concluiu : AcaoHistorico.Cancelou,
                concluiu ? $"Pedido #{id} concluído." : $"Pedido #{id} cancelado; os itens voltaram ao estoque.");

            return Results.Ok("Status atualizado com sucesso.");
        });


        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id, HistoricoService historico, HttpContext ctx) =>
        {
            var excluido = await PedidoService.ExcluirPedido(id);

            if (!excluido)
                return Resposta.Erro("Não foi possível excluir o pedido.");

            await historico.Registrar(ctx, EntidadeHistorico.Pedido, id, AcaoHistorico.Excluiu,
                $"Pedido #{id} excluído.");

            return Results.NoContent();
        });
    }

    // ---------- MAPEAR ----------
    private static object Mapear(Pedido p) => new
    {
        p.Id,
        p.ClienteId,
        ClienteNome = p.Cliente?.Nome ?? "Usuário removido",
        Data = p.Data == default ? DateTime.Now : p.Data,
        p.Status,
        Total = p.Itens.Sum(i => i.PrecoUnitario * i.Quantidade),
        Itens = p.Itens.Select(i => new
        {
            i.ProdutoId,
            Nome = i.Produto?.Nome ?? "Produto",
            Preco = i.PrecoUnitario,
            i.Quantidade
        }).ToList()
    };
}
