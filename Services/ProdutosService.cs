using Models;
using Validators;
namespace Services;

using Microsoft.EntityFrameworkCore;
using PI_1ANO.Data;
public class ProdutoService{
    public static async Task<Produto> CreateProduto(string nome, decimal preco, int estoque)
    {
        await using var db = new AppDbContext();
        var produto = new Produto
        {
            Nome = nome,
            Preco = preco,
            Estoque = estoque
        };
        db.Produtos.Add(produto);
        await db.SaveChangesAsync();
        return produto;
    }
    public static async Task Estocar(int id, int estoque)
    {
        await using var db = new AppDbContext();
        var produto = await db.Produtos.FindAsync(id);
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado");
            return;
        }
        if (estoque <= 0)
        {
            Console.WriteLine("No estoque não pode ser aticionado negativo ou zero");
            return;
        }
        produto.Estoque =+ estoque;
        await db.SaveChangesAsync();
    }
    public static async Task<List<Produto>> ListarProdutos()
    {
        await using var db = new AppDbContext();
        return await db.Produtos.ToListAsync();
    }
    public static async Task<List<Produto>> BuscarPorNome(string nome)
    {
        await using var db = new AppDbContext();
        var resultado = db.Produtos.Where(p => EF.Functions.Like(p.Nome, $"%{nome}%")).ToList();
        if (resultado == null)
        {
            Console.WriteLine("Produto não encontrado");
            return null;
        }
        return resultado;
    }
    public static async Task AtualizarProdutos(int id, string nome, decimal preco, int estoque)
    {
        await using var db = new AppDbContext();
        var produto = await db.Produtos.FindAsync(id);
    }
}