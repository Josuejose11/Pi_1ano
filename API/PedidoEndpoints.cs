
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
            var lista = await PedidoService.ListarPedidos(clienteId);

            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (NovoPedidoDto dto) =>
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

            return Results.Ok(pedido);
        });

        // ---------- UPDATE (status) ----------
        rotas.MapPut("/{id:int}/status", async (int id, StatusDto dto) =>
        {
            if (dto.Status == null)
                return Resposta.Erro("Status inválido.");

            var atualizado = await PedidoService.AtualizarStatus(id, dto.Status);

            if (!atualizado)
                return Resposta.Erro("Não foi possível atualizar o pedido.");

            return Results.Ok("Status atualizado com sucesso.");
        });

        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id) =>
        {
            var excluido = await PedidoService.ExcluirPedido(id);

            if (!excluido)
                return Resposta.Erro("Não foi possível excluir o pedido.");

            return Results.NoContent();
        });
    }

    // ---------- MAPEAR ----------
    private static object Mapear(Pedido p) => new
    {
        p.Id,
        p.UsuarioId,
        UsuarioNome = p.Usuario?.Nome ?? "Usuário removido",
        p.Data,
        p.Status,
        Total = p.Itens.Sum(i => i.Preco * i.Quantidade),
        Itens = p.Itens.Select(i => new
        {
            i.ProdutoId,
            Nome = i.Produto?.Nome ?? i.Nome,
            i.Preco,
            i.Quantidade
        }).ToList()
    };
}
