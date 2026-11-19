using Models;
using Validators;
namespace Services;
using Microsoft.EntityFrameworkCore;
using PI_1ANO.Data;
public class ProdutoService{
    private readonly ProdutoValidator _validator;

    public ProdutoService(ProdutoValidator validator)
    {
        _validator = validator;
    }
    // Cadastra o produto
    public async Task<bool> CreateProduto(Produto produto)
    {
        if (!_validator.ValidarProduto(produto))
            return false;
        await using var db = new AppDbContext();
        db.Produtos.Add(produto);
        Console.WriteLine($"Produto de ID: {produto.Id} | Produto: {produto.Nome} adicionado com sucesso");
        await db.SaveChangesAsync();
        return true;
    }
    
    // Lista todos os produtos
    public static async Task<List<Produto>> ListarProdutos()
    {
        await using var db = new AppDbContext();
        var resultado = await db.Produtos.ToListAsync();
        if (resultado.Count == 0)
        {
            Console.WriteLine("Produto não encontrado");
        }
        return resultado;
    }
    
    // Busca produto por nome
    public static async Task<List<Produto>> BuscarPorNome(string nome)
    {
        await using var db = new AppDbContext();
        var resultado = db.Produtos.Where(p => EF.Functions.Like(p.Nome, $"%{nome}%")).ToList();
        if (resultado.Count == 0)
        {
            Console.WriteLine("Produto não encontrado");
        }
        return resultado;
    }
    
    // altera o nome do produto
    public async Task<bool> AlterarNome(int id, string nome)
    {
        await using var db = new AppDbContext();
        var produto = await db.Produtos.FindAsync(id);
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado");
            return false;
        }
        if (!_validator.ValidarNome(nome))
            return false;
        produto.Nome = nome;
        await db.SaveChangesAsync();
        Console.WriteLine($"ID: {id} - Produto: {nome} foi alterado com sucesso");
        return true;
    }
    
    // Atualiza o preço do produto
    public async Task<bool> AtualizarPreco(int id, decimal preco)
    {
        await using var db = new AppDbContext();
        var produto = await db.Produtos.FindAsync(id);
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado");
            return false;
        }
        if (!_validator.ValidarPreco(preco))
            return false;
        produto.Preco = preco;
        await db.SaveChangesAsync();
        Console.WriteLine($"ID: {id} - Preço: {preco} foi alterado com sucesso");
        return true;
    }
    
    // Adiciona ao estoque do produto
    public async Task<bool> Estocar(int id, int entrada)
    {
        await using var db = new AppDbContext();
        if (!_validator.ValidarEstoque(entrada))
            return false;
        var produto = await db.Produtos.FindAsync(id);
        if (produto == null)
        {
            Console.WriteLine("Produto não encontrado");
            return false;
        }
        produto.Estoque += entrada;
        await db.SaveChangesAsync();
        Console.WriteLine($"ID: {id} - Foi adicionado ao estoque: {entrada} com sucesso");
        return true;
    }
    
    // Deleta o produto
    public static async Task DeletarProduto(int id)
    {
        await using var db = new AppDbContext();
        var produto = await db.Produtos.FindAsync(id);
        if(produto == null)
        {
            Console.WriteLine("Produto não encontrado");
            return;
        }
        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();
    }
}