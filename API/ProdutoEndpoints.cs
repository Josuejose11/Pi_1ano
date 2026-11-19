using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;

namespace Api;

public record ProdutoDto(string? Nome, decimal? Preco, int? Estoque);
public record EstoqueDto(int? Quantidade);

// CRUD de produtos:
//   GET    /api/produtos                 -> lista todos          (Read)
//   POST   /api/produtos                 -> cria                 (Create)
//   PUT    /api/produtos/{id}            -> edita                (Update)
//   DELETE /api/produtos/{id}            -> exclui               (Delete)
//   POST   /api/produtos/{id}/estoque    -> soma unidades ao estoque
public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this IEndpointRouteBuilder app)
    {
        // MapGroup: todas as rotas abaixo começam com /api/produtos
        var rotas = app.MapGroup("/api/produtos");

        // ---------- READ ----------
        rotas.MapGet("/", async (AppDbContext db) =>
        {
            var lista = await db.Produtos
                .OrderBy(p => p.Nome)
                .Select(p => new { p.Id, p.Nome, p.Preco, p.Estoque })
                .ToListAsync();
            return Results.Ok(lista); // vira JSON: [{"id":1,"nome":"...","preco":24.9,"estoque":18}, ...]
        });

        // ---------- CREATE ----------
        rotas.MapPost("/", async (ProdutoDto dto, AppDbContext db) =>
        {
            string? erro = Validar(dto);
            if (erro != null) return Resposta.Erro(erro);

            var produto = new Produto
            {
                Nome = LimparEspacos(dto.Nome!),
                Preco = Math.Round(dto.Preco!.Value, 2),
                Estoque = dto.Estoque
            };
            db.Produtos.Add(produto);
            await db.SaveChangesAsync(); // aqui o INSERT acontece de fato no MySQL
            return Results.Ok(Mapear(produto));
        });

        // ---------- UPDATE ----------
        rotas.MapPut("/{id:int}", async (int id, ProdutoDto dto, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null) return Resposta.NaoEncontrado("Produto não encontrado.");

            string? erro = Validar(dto);
            if (erro != null) return Resposta.Erro(erro);

            produto.Nome = LimparEspacos(dto.Nome!);
            produto.Preco = Math.Round(dto.Preco!.Value, 2);
            produto.Estoque = dto.Estoque;
            await db.SaveChangesAsync(); // o EF percebe o que mudou e faz só o UPDATE necessário
            return Results.Ok(Mapear(produto));
        });

        // ---------- DELETE ----------
        rotas.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null) return Resposta.NaoEncontrado("Produto não encontrado.");

            // Produto que já foi vendido não pode sumir: os pedidos antigos dependem dele.
            bool temPedidos = await db.ItensPedido.AnyAsync(i => i.ProdutoId == id);
            if (temPedidos)
                return Resposta.Erro("Este produto aparece em pedidos e não pode ser excluído. Zere o estoque para tirá-lo da venda.", 409);

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();
            return Results.NoContent(); // 204: deu certo, não há nada para devolver
        });

        // ---------- ESTOCAR (soma ao estoque) ----------
        rotas.MapPost("/{id:int}/estoque", async (int id, EstoqueDto dto, AppDbContext db) =>
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null) return Resposta.NaoEncontrado("Produto não encontrado.");

            if (dto.Quantidade is null || dto.Quantidade <= 0)
                return Resposta.Erro("A quantidade a adicionar precisa ser maior que zero.");

            // "+" e não "=": soma ao que já existe (estoque 10 + 5 = 15).
            produto.Estoque = (produto.Estoque ?? 0) + dto.Quantidade.Value;
            await db.SaveChangesAsync();
            return Results.Ok(Mapear(produto));
        });
    }

    // Devolve a mensagem de erro, ou null se estiver tudo certo.
    private static string? Validar(ProdutoDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Nome)) return "Informe o nome do produto.";
        if (d.Preco is null || d.Preco < 0) return "Informe um preço válido (zero ou maior).";
        if (d.Estoque is null || d.Estoque < 0) return "O estoque precisa ser um número inteiro, zero ou maior.";
        return null;
    }

    // "  Café   em grão " -> "Café em grão"
    private static string LimparEspacos(string texto)
        => string.Join(" ", texto.Split(' ', StringSplitOptions.RemoveEmptyEntries));

    // Monta só os campos que o HTML precisa (evita devolver a entidade inteira do banco).
    private static object Mapear(Produto p) => new { p.Id, p.Nome, p.Preco, p.Estoque };
}
