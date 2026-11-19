namespace Models;

public class Pedido
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
    public int ClienteId { get; set; }
    public Usuario Cliente { get; set; } = null!;
    public List<ItemPedido> Itens { get; set; } = new();
    public void adicionarItem(Produto produto, int quantidade)
    {
        if (quantidade <= 0)
        {
            Console.WriteLine("A quatidade pedida não pode ser negativa ou zero");
            return;
        }
    }
}
public class ItemPedido
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
}