using Models;
using Validators;
namespace Services;
using Microsoft.EntityFrameworkCore;
using PI_1ANO.Data;
public class PedidoService{
    // CREATE 
    public static async Task<Pedido?> CreatePedido(int usuarioId, List<ItemPedido> ItensPedidos)
    {
        await using var db = new AppDbContext();
        var usuario = await db.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            Console.WriteLine("Usuario não encontrado");
            return null;
        }
        var pedido = new Pedido{UsuarioId = usuarioId};
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
        var resultado = await db.Pedidos.Include(p => p.Usuario).Include(p => p.Itens).ThenInclude(i => i.Produto).ToListAsync();
        if (resultado.Count == 0)
        {
            Console.WriteLine("Pedido não encontrado");
        }
        return resultado;
    }
    public static async Task CancelarPedido(int pedidoId)
    {
        await using var db = new AppDbContext();
        var pedido = await db.Pedidos.Include(p => p.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(p => p.Id == pedidoId);
        if (pedido == null)
        {
            Console.WriteLine("Pedido não encontrado");
            return;
        }
        pedido.Cancelar();
        foreach (var item in pedido.Itens)
        {
            item.Produto.Estoque += item.Quantidade;
        }
        await db.SaveChangesAsync();
        Console.WriteLine($"Pedido de ID: {pedidoId} cancelado, itens voltaram ao estoque");
    }
}