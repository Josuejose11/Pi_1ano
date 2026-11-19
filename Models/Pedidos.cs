using Org.BouncyCastle.Asn1.Cmp;

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
        var existe = Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (existe != null)
        {
            existe.Quantidade += quantidade;
        }
        else
        {
            Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = quantidade,
                PrecoUnitario = produto.Preco
            });
        }
        CalcularValorTotal();
    }
    public void CalcularValorTotal()
    {
        ValorTotal = Itens.Sum(i => i.Quantidade * i.PrecoUnitario);
    }
    public void Cancelar()
    {
        if (Status == "Cancelado")
        {
            Console.WriteLine("Esse pedido já está cancelado");
        }
        else
        {
            Status = "Cancelado";
        }
    }
    public void Aprovar()
    {
        if (Status == "Aprovado")
        {
            Console.WriteLine("Esse pedido já está aprovado");
        }
        else
        {
            Status = "Aprovado";
        }
    }
}
public class ItemPedido
{
    public int Id { get; set; }
    public int Quantidade { get; set; }
    public decimal? PrecoUnitario { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
}