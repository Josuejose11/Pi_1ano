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
    public DbSet<Historico> Historico { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(
            @"Data Source=sistema_pedidos.db"
        );
    }

    // Configurando os relacionamentos entre as entidades
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cliente e Gestor ficam na mesma tabela. A coluna "Tipo" recebe "cliente" ou "gestor".
        modelBuilder.Entity<Usuario>(u =>
        {
            // E-mail único: não deixa duas contas com o mesmo e-mail.
            u.Property(x => x.Email).HasMaxLength(150);
            // O MaxLength é obrigatório no MySQL para poder criar um índice em texto.
            u.HasIndex(x => x.Email).IsUnique();
        });


        // Restrict = o banco recusa apagar um cliente que tem pedidos
        // (ou um produto que aparece em pedidos). Protege o histórico de vendas.
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ItemPedido>()
            .HasOne(i => i.Produto)
            .WithMany()
            .HasForeignKey(i => i.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}