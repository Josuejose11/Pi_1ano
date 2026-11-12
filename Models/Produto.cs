using PI_1ANO.Data;
using Microsoft.EntityFrameworkCore;
namespace PI_1ANO.Models;
public class Produto
{
    public int Id { get; set; }
    public string? Nome { get; set; } =string.Empty;
    public decimal? Preco { get; set; }
    public int? Estoque { get; set; }

}