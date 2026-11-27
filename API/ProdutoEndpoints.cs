
using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;

namespace Api;

public record ProdutoDto(string? Nome, decimal Preco, int Estoque);
public record EstoqueDto(int? Quantidade);

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/produtos");

        // ---------- READ ----------
        rotas.MapGet("/", async (ProdutoService service) =>
        {
            var lista = await service.ListarProdutos();

            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- BUSCAR POR NOME ----------
        rotas.MapGet("/buscar", async (string nome, ProdutoService service) =>
        {
            var lista = await service.BuscarPorNome(nome);

            return Results.Ok(lista.Select(Mapear));
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (ProdutoDto dto, ProdutoService service, HistoricoService historico, HttpContext ctx) =>
        {
            var produto = new Produto
            (
                dto.Nome,
                dto.Preco,
                dto.Estoque
            );

            bool criado = await service.CreateProduto(produto);

            if (!criado)
                return Resposta.Erro("Não foi possível cadastrar o produto. Verifique os dados.");

            await historico.Registrar(ctx, EntidadeHistorico.Produto, produto.Id, AcaoHistorico.Criou,
                $"Produto \"{produto.Nome}\" cadastrado ({TextoHistorico.Moeda(produto.Preco)}, estoque {produto.Estoque}).");

            return Results.Ok(Mapear(produto));
        });

        // ---------- UPDATE ----------
        rotas.MapPut("/{id:int}", async (int id, ProdutoDto dto, ProdutoService service, HistoricoService historico, HttpContext ctx, AppDbContext db) =>
        {
            // Guarda como o produto era ANTES, para o histórico dizer o que mudou.
            var antes = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            var dados = new Produto
            (
                dto.Nome,
                dto.Preco,
                dto.Estoque
            );

            bool atualizado = await service.AtualizarProduto(id, dados);

            if (!atualizado)
                return Resposta.Erro("Não foi possível atualizar o produto.");

            var mudancas = new List<string>();
            if (antes != null)
            {
                if (antes.Nome != dados.Nome) mudancas.Add($"nome \"{antes.Nome}\" → \"{dados.Nome}\"");
                if (antes.Preco != dados.Preco) mudancas.Add($"preço {TextoHistorico.Moeda(antes.Preco)} → {TextoHistorico.Moeda(dados.Preco)}");
                if (antes.Estoque != dados.Estoque) mudancas.Add($"estoque {antes.Estoque} → {dados.Estoque}");
            }
            await historico.Registrar(ctx, EntidadeHistorico.Produto, id, AcaoHistorico.Editou,
                $"Produto \"{dados.Nome}\" editado" + (mudancas.Count > 0 ? ": " + string.Join(", ", mudancas) + "." : "."));

            return Results.Ok("Produto atualizado com sucesso.");
        });

        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id, ProdutoService service, HistoricoService historico, HttpContext ctx, AppDbContext db) =>
        {
            // Pega o nome ANTES de apagar (depois que apaga, não dá mais para saber).
            string nome = (await db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id))?.Nome ?? "";
            bool excluido = await service.DeletarProduto(id);

            if (!excluido)
                return Resposta.Erro("Não foi possível excluir o produto. Ele pode não existir ou estar vinculado a pedidos.");

            await historico.Registrar(ctx, EntidadeHistorico.Produto, id, AcaoHistorico.Excluiu,
                $"Produto \"{nome}\" excluído.");

            return Results.NoContent();
        });

        // ---------- ESTOCAR ----------
        rotas.MapPost("/{id:int}/estoque", async (int id, EstoqueDto dto, ProdutoService service, HistoricoService historico, HttpContext ctx, AppDbContext db) =>
        {
            if (dto.Quantidade == null)
                return Resposta.Erro("Informe uma quantidade válida.");

            bool estocado = await service.Estocar(id, dto.Quantidade.Value);

            if (!estocado)
                return Resposta.Erro("Não foi possível adicionar ao estoque. Verifique o produto e a quantidade.");

            var produto = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            await historico.Registrar(ctx, EntidadeHistorico.Produto, id, AcaoHistorico.Estocou,
                $"Entraram {dto.Quantidade} unidade(s) de \"{produto?.Nome}\". Estoque agora: {produto?.Estoque}.");

            return Results.Ok("Estoque atualizado com sucesso.");
        });
    }

    // ---------- MAPEAR ----------
    private static object Mapear(Produto p) => new
    {
        p.Id,
        p.Nome,
        p.Preco,
        p.Estoque
    };
}
