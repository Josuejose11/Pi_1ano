using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;

namespace Services;

// Padronizacao das palavras 
public static class EntidadeHistorico
{
    public const string Produto = "Produto";
    public const string Pedido = "Pedido";
    public const string Usuario = "Usuário";
}

public static class AcaoHistorico
{
    public const string Criou = "Criou";
    public const string Editou = "Editou";
    public const string Excluiu = "Excluiu";
    public const string Estocou = "Estocou";
    public const string Concluiu = "Concluiu";
    public const string Cancelou = "Cancelou";
    public const string Entrou = "Entrou";
}

// O que a tela recebe: o registro do banco + o nome de quem fez a ação.
public record HistoricoItem(
    int Id,
    int UsuarioId,
    string UsuarioNome,
    string UsuarioTipo,
    string TipoEntidade,
    int EntidadeId,
    string Acao,
    string? Descricao,
    DateTime DataHora);


// Filtros aceitos na listagem. Tudo é opcional.
public record HistoricoFiltro(
    int? UsuarioId = null,       // só as ações desse usuário
    string? Nome = null,         // parte do nome de quem fez a ação
    string? TipoEntidade = null, // Produto, Pedido ou Usuário
    string? Acao = null,         // Criou, Editou, Excluiu...
    DateTime? De = null,         // a partir desta data
    DateTime? Ate = null,        // até o fim desta data
    int Limite = 200);           // máximo de linhas devolvidas


// Classe de service pro historico 
public class HistoricoService
{
    // Registra acao no historico 
    public async Task<bool> RegistrarHistorico(
        int usuarioId,
        string tipoEntidade,
        int entidadeId,
        string acao,
        string? descricao = null)
    {
        try
        {
            await using var db = new AppDbContext();
            db.Historico.Add(new Historico(usuarioId, tipoEntidade, entidadeId, acao, descricao));
            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Não foi possível gravar o histórico: " + ex.Message);
            return false;
        }
    }

    // Read geral
    public async Task<List<HistoricoItem>> Listar(HistoricoFiltro filtro)
    {
        await using var db = new AppDbContext();
        IQueryable<Historico> consulta = db.Historico;

        if (filtro.UsuarioId.HasValue)
            consulta = consulta.Where(h => h.UsuarioId == filtro.UsuarioId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Nome))
        {
            // TODOS os usuários cujo nome contém o texto; depois pega as ações deles.
            string nome = filtro.Nome.Trim();
            var ids = await db.Usuarios
                .Where(u => EF.Functions.Like(u.Nome, $"%{nome}%"))
                .Select(u => u.Id)
                .ToListAsync();
            consulta = consulta.Where(h => ids.Contains(h.UsuarioId));
        }

        if (!string.IsNullOrWhiteSpace(filtro.TipoEntidade))
            consulta = consulta.Where(h => h.TipoEntidade == filtro.TipoEntidade);

        if (!string.IsNullOrWhiteSpace(filtro.Acao))
            consulta = consulta.Where(h => h.Acao == filtro.Acao);

        if (filtro.De.HasValue)
            consulta = consulta.Where(h => h.DataHora >= filtro.De.Value.Date);

        if (filtro.Ate.HasValue)
            consulta = consulta.Where(h => h.DataHora < filtro.Ate.Value.Date.AddDays(1));

        int limite = Math.Clamp(filtro.Limite, 1, 1000);
        var registros = await consulta
            .OrderByDescending(h => h.DataHora)
            .ThenByDescending(h => h.Id)
            .Take(limite)
            .ToListAsync();

        // Busca os nomes dos usuários de uma vez (uma consulta só, não uma por linha).
        var idsUsuarios = registros.Select(h => h.UsuarioId).Distinct().ToList();
        var usuarios = await db.Usuarios
            .Where(u => idsUsuarios.Contains(u.Id))
            .Select(u => new { u.Id, u.Nome, u.Tipo })
            .ToDictionaryAsync(u => u.Id);

        return registros.Select(h =>
        {
            usuarios.TryGetValue(h.UsuarioId, out var u);
            return new HistoricoItem(
                h.Id,
                h.UsuarioId,
                u?.Nome ?? (h.UsuarioId == 0 ? "Sistema" : "Usuário removido"),
                (u?.Tipo ?? "").ToLower(),
                h.TipoEntidade,
                h.EntidadeId,
                h.Acao,
                h.Descricao,
                h.DataHora);
        }).ToList();
    }

    
    // Antes só olhava o PRIMEIRO usuário que combinava com o nome; agora considera todos.
    public Task<List<HistoricoItem>> BuscarPorNome(string nomeUsuario)
        => Listar(new HistoricoFiltro(Nome: nomeUsuario));
}