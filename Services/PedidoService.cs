using Models;
using Validators;
namespace Services;
using Microsoft.EntityFrameworkCore;
using PI_1ANO.Data;
public class PedidoService{
    // CREATE 
    public static async Task<Pedido?> CreatePedido(int clienteId, List<ItemPedido> ItensPedidos)
    {
        await using var db = new AppDbContext();
        var usuario = await db.Usuarios.FindAsync(clienteId);
        if (usuario == null)
        {
            Console.WriteLine("Usuario não encontrado");
            return null;
        }
        var pedido = new Pedido{ClienteId = clienteId};
        foreach (var item in ItensPedidos)
        {
            var produto = await db.Produtos.FindAsync(item.ProdutoId);
            if (produto == null)
            {
                Console.WriteLine($"Produto de ID: {item.ProdutoId} não encontrado");
                return null;
            } 
            if (produto.Estoque < item.Quantidade)
            {
                Console.WriteLine($"Produto de ID: {item.ProdutoId} tem estoque insuficiente");
                return null;
            }
                produto.Estoque -= item.Quantidade;
                pedido.adicionarItem(produto, item.Quantidade);
        }
        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();
        return pedido;
    }

    // READ 
    public static async Task<List<Pedido>> ListarPedidos()
    {
        await using var db = new AppDbContext();
        var resultado = await db.Pedidos.Include(p => p.Cliente).Include(p => p.Itens).ThenInclude(i => i.Produto).ToListAsync();
        if (resultado.Count == 0)
        {
            Console.WriteLine("Pedido não encontrado");
        }
        return resultado;
    }
    
    // Mudar status do pedido para cancelado 
    public static async Task<bool> CancelarPedido(int pedidoId)
    {
        await using var db = new AppDbContext();
        var pedido = await db.Pedidos.Include(p => p.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(p => p.Id == pedidoId);
        if (pedido == null)
        {
            Console.WriteLine("Pedido não encontrado");
            return false;
        }
        pedido.Cancelar();
        foreach (var item in pedido.Itens)
        {
            item.Produto.Estoque += item.Quantidade;
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"Pedido de ID: {pedidoId} cancelado, itens voltaram ao estoque");
        return true;
    }

    // DELETE 
    public static async Task<bool> ExcluirPedido(int id)
    {
        await using var db = new AppDbContext();
        var pedido = await db.Pedidos.Include(p => p.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido == null)
        {
            Console.WriteLine("Pedido não encontrado");
            return false;
        }
        if (pedido.Status != "Cancelado")
        {
            foreach (var item in pedido.Itens)
            {
                item.Produto.Estoque += item.Quantidade;
            }
            Console.WriteLine("Itens voltaram ao estoque");
        }
        Console.WriteLine($"Pedido de ID: {id} excluindo");
        db.Pedidos.Remove(pedido);
        await db.SaveChangesAsync();
        return true;
    }
    public static async Task<bool> AtualizarStatus(int pedidoId, string novoStatus)
    {
        await using var db = new AppDbContext();
        var pedido = await db.Pedidos
        .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
        .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            return false;

        if (pedido.Status == "Cancelado" || pedido.Status == "Concluído")
            return false;

        if (novoStatus == "Concluído")
        {
            pedido.Status = "Concluído";
        }
        else if (novoStatus == "Cancelado")
        {
            pedido.Status = "Cancelado";
            foreach (var item in pedido.Itens)
            {
                item.Produto.Estoque += item.Quantidade;
            }
        }
        else
        {
            return false;
        }
        await db.SaveChangesAsync();
        return true;
    }

}