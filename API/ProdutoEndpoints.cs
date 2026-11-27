
using Models;
using Services;

namespace Api;

public record ProdutoDto(string Nome, decimal Preco, int Estoque);
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
        rotas.MapPost("/", async (ProdutoDto dto, ProdutoService service) =>
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

            return Results.Ok(Mapear(produto));
        });

        // ---------- UPDATE ----------
        rotas.MapPut("/{id:int}", async (int id, ProdutoDto dto, ProdutoService service) =>
        {
            var dados = new Produto
            (
                dto.Nome,
                dto.Preco,
                dto.Estoque
            );

            bool atualizado = await service.AtualizarProduto(id, dados);

            if (!atualizado)
                return Resposta.Erro("Não foi possível atualizar o produto.");

            return Results.Ok("Produto atualizado com sucesso.");
        });

        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id, ProdutoService service) =>
        {
            bool excluido = await service.DeletarProduto(id);

            if (!excluido)
                return Resposta.Erro("Não foi possível excluir o produto. Ele pode não existir ou estar vinculado a pedidos.");

            return Results.NoContent();
        });

        // ---------- ESTOCAR ----------
        rotas.MapPost("/{id:int}/estoque", async (int id, EstoqueDto dto, ProdutoService service) =>
        {
            if (dto.Quantidade == null)
                return Resposta.Erro("Informe uma quantidade válida.");

            bool estocado = await service.Estocar(id, dto.Quantidade.Value);

            if (!estocado)
                return Resposta.Erro("Não foi possível adicionar ao estoque. Verifique o produto e a quantidade.");

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
