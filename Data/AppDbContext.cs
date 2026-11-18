using Models;
using Microsoft.EntityFrameworkCore;

namespace PI_1ANO.Data;

public class AppDbContext : DbContext
{
    //classes aqui
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedido { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(
            @"server=localhost;
            database=GerenciadorDeEstoqueEVenda;
            user=root;
            Password=Senac2026;"
        );
    }
}