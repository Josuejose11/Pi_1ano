using System.Runtime.CompilerServices;
using Models;
using PI_1ANO.Data;
using Microsoft.EntityFrameworkCore;


namespace Services;


public class HistoricoService
{
    // Método de registrar historico
    public async Task<bool> RegistrarHistorico(
    int usuarioId,
    string tipoEntidade,
    int entidadeId,
    string acao,
    string? descricao = null)
    {
        await using var db = new AppDbContext();
        var historico = new Historico
        (
            usuarioId,
            tipoEntidade,
            entidadeId,
            acao,
            descricao
        );
        historico.DataHora = DateTime.Now;
        db.Historico.Add(historico);
        await db.SaveChangesAsync();
        return true;
    }

    
    // Ler historico 
    public async Task<List<Historico>> VerHistorico()
    {
        await using var db = new AppDbContext();
        var resultado = await db.Historico
            .OrderByDescending(h => h.DataHora)
            .ToListAsync();

        if (resultado.Count == 0)
        {
            Console.WriteLine("Historico não encontrado");
        }
        return resultado;
    }

    // Buscar por nome do usuario
    public async Task<List<Historico>> BuscarPorNome(string nomeUsuario)
    {
        await using var db = new AppDbContext();
        var resultado = await db.Historico
            .Where(h => h.UsuarioId == db.Usuarios
                .Where(u => EF.Functions.Like(u.Nome, $"%{nomeUsuario}%"))
                .Select(u => u.Id)
                .FirstOrDefault())
            .OrderByDescending(h => h.DataHora)
            .ToListAsync();

        if (resultado.Count == 0)
        {
            Console.WriteLine("Historico não encontrado");
        }
        return resultado;
    }
}