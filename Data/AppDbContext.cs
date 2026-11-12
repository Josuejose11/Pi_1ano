//using PI_1ANO.Models; colocar o namespace no models
using Microsoft.EntityFrameworkCore;

namespace PI_1ANO.Data;

public class AppDbContext : DbContext
{
    //classes aqui

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL(
            @"server=localhost;
            database=//nomeBancoAqui;
            user=root;
            Password=Senac2026;"
        );
    }
}