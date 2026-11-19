using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;

namespace Api;

public record PedidoItemDto(int ProdutoId, int Quantidade);
public record NovoPedidoDto(int ClienteId, List<PedidoItemDto>? Itens);
public record StatusDto(string? Status);

// CRUD de pedidos:
//   GET    /api/pedidos?clienteId=2     -> lista (clienteId é opcional: sem ele, lista de todos)
//   POST   /api/pedidos                 -> cria o pedido e DESCONTA o estoque
//   PUT    /api/pedidos/{id}/status     -> concluir ou cancelar (cancelar DEVOLVE o estoque)
//   DELETE /api/pedidos/{id}            -> exclui (só pedidos cancelados)
public static class PedidoEndpoints
{
    public static void MapPedidoEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/pedidos");

        // ---------- READ ----------
        rotas.MapGet("/", async (int? clienteId, AppDbContext db) =>
        {
            // Include = "traga junto" o cliente e os itens (senão viriam vazios).
            IQueryable<Pedido> consulta = db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens);

            // Cliente comum vê só os seus pedidos; o gestor chama sem clienteId e vê todos.
            if (clienteId != null)
                consulta = consulta.Where(p => p.ClienteId == clienteId);

            var lista = await consulta.OrderByDescending(p => p.Id).ToListAsync();
            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (NovoPedidoDto dto, AppDbContext db) =>
        {
            if (dto.Itens == null || dto.Itens.Count == 0)
                return Resposta.Erro("O carrinho está vazio.");

            var cliente = await db.Usuarios.OfType<Usuario>().FirstOrDefaultAsync(c => c.Id == dto.ClienteId);
            if (cliente == null) return Resposta.NaoEncontrado("Cliente não encontrado.");

            // Se o mesmo produto veio em duas linhas, junta numa só somando as quantidades.
            var linhas = dto.Itens
                .GroupBy(i => i.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(i => i.Quantidade) })
                .ToList();

            // Uma única consulta traz todos os produtos do carrinho.
            var ids = linhas.Select(l => l.ProdutoId).ToList();
            var produtos = await db.Produtos.Where(p => ids.Contains(p.Id)).ToListAsync();

            var pedido = new Pedido { Cliente = cliente, Data = DateTime.Now, Status = StatusPedido.Pendente };

            foreach (var linha in linhas)
            {
                var produto = produtos.FirstOrDefault(p => p.Id == linha.ProdutoId);
                if (produto == null) return Resposta.Erro("Um produto do carrinho não existe mais.");
                if (linha.Quantidade < 1) return Resposta.Erro("Quantidade inválida.");

                int estoque = produto.Estoque ?? 0;
                if (estoque < linha.Quantidade)
                    return Resposta.Erro($"Estoque insuficiente de {produto.Nome}: restam {estoque}.");

                produto.Estoque = estoque - linha.Quantidade; // desconta do estoque

                // Copia nome e preço do produto para o item (o "retrato" da venda).
                pedido.Itens.Add(new ItemPedido
                {
                    ProdutoId = produto.Id,
                    Nome = produto.Nome ?? "",
                    Preco = produto.Preco ?? 0,
                    Quantidade = linha.Quantidade
                });
            }

            // Um único SaveChanges grava o pedido, os itens e o novo estoque juntos,
            // dentro de uma transação: ou grava tudo, ou não grava nada.
            // (Se alguma validação acima falhou, o "return" saiu antes e NADA foi gravado.)
            db.Pedidos.Add(pedido);
            await db.SaveChangesAsync();
            return Results.Ok(Mapear(pedido));
        });

        // ---------- UPDATE (status) ----------
        rotas.MapPut("/{id:int}/status", async (int id, StatusDto dto, AppDbContext db) =>
        {
            var pedido = await db.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return Resposta.NaoEncontrado("Pedido não encontrado.");

            // Só o pedido Pendente pode mudar. Concluído e Cancelado são finais.
            if (pedido.Status != StatusPedido.Pendente)
                return Resposta.Erro($"Pedido {pedido.Status.ToLower()} não pode mudar de status.");

            if (dto.Status != StatusPedido.Concluido && dto.Status != StatusPedido.Cancelado)
                return Resposta.Erro("Status inválido.");

            // Cancelar = devolver ao estoque tudo o que o pedido tinha tirado.
            if (dto.Status == StatusPedido.Cancelado)
            {
                foreach (var item in pedido.Itens)
                {
                    var produto = await db.Produtos.FindAsync(item.ProdutoId);
                    if (produto != null)
                        produto.Estoque = (produto.Estoque ?? 0) + item.Quantidade;
                }
            }

            pedido.Status = dto.Status!;
            await db.SaveChangesAsync();
            return Results.Ok(Mapear(pedido));
        });

        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var pedido = await db.Pedidos.Include(p => p.Itens).FirstOrDefaultAsync(p => p.Id == id);
            if (pedido == null) return Resposta.NaoEncontrado("Pedido não encontrado.");

            // Pedido concluído é venda feita: faz parte do histórico e não se apaga.
            if (pedido.Status != StatusPedido.Cancelado)
                return Resposta.Erro("Só dá para excluir pedidos cancelados.");

            db.Pedidos.Remove(pedido); // os itens vão junto (exclusão em cascata)
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // Formato que o HTML espera:
    // {id, clienteId, clienteNome, data, status, total, itens:[{produtoId, nome, preco, quantidade}]}
    private static object Mapear(Pedido p) => new
    {
        p.Id,
        p.ClienteId,
        ClienteNome = p.Cliente?.Nome ?? "Cliente removido",
        p.Data,
        p.Status,
        Total = p.Itens.Sum(i => i.Preco * i.Quantidade),
        Itens = p.Itens.Select(i => new { i.ProdutoId, i.Nome, i.Preco, i.Quantidade }).ToList()
    };
}
