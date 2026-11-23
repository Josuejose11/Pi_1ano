using Models;
using Validators;
using Microsoft.EntityFrameworkCore;
using PI_1ANO.Data;

namespace Services;

public class ProdutoService{
    private readonly ProdutoValidator _validator;

    public ProdutoService(ProdutoValidator validator)
    {
        _validator = validator;
    }

    // CREATE - Cadastra o produto
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
    public async Task<List<Produto>> ListarProdutos()
    {
        await using var db = new AppDbContext();
        var resultado = await db.Produtos
        .OrderBy(p => p.Nome)
        .ToListAsync();
        if (resultado.Count == 0)
        {
            Console.WriteLine("Produto não encontrado");
        }
        return resultado;
    }
    
    // Busca produto por nome
    public async Task<List<Produto>> BuscarPorNome(string nome)
    {
        await using var db = new AppDbContext();
        var resultado = db.Produtos
        .Where(p => EF.Functions.Like(p.Nome, $"%{nome}%"))
        .OrderBy(p => p.Nome)
        .ToList();
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
    
    // UPDATE - Adiciona unidades ao estoque
    public async Task<bool> Estocar(int id, int entrada)
    {
        if (!_validator.ValidarEstoque(entrada))
            return false;

        await using var db = new AppDbContext();

        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
            return false;

        produto.Estoque = produto.Estoque + entrada;

        await db.SaveChangesAsync();

        Console.WriteLine($"Foram adicionadas {entrada} unidades ao produto {id}.");

        return true;
    }

    
    // DELETE - Exclui produto
    public async Task<bool> DeletarProduto(int id)
    {
        await using var db = new AppDbContext();

        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
            return false;

        // Não permite excluir produtos que aparecem em pedidos.
        bool temPedidos = await db.ItensPedido
            .AnyAsync(i => i.ProdutoId == id);

        if (temPedidos)
            return false;

        db.Produtos.Remove(produto);

        await db.SaveChangesAsync();

        return true;
    }

     // UPDATE - Atualiza nome, preço e estoque
    public async Task<bool> AtualizarProduto(int id, Produto dados)
    {
        await using var db = new AppDbContext();

        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
            return false;

        if (!_validator.ValidarProduto(dados))
            return false;

        produto.Nome = dados.Nome;
        produto.Preco = dados.Preco;
        produto.Estoque = dados.Estoque;

        await db.SaveChangesAsync();

        Console.WriteLine($"Produto ID {id} atualizado com sucesso.");

        return true;
    }

}