using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Models;
using PI_1ANO.Data;
using Services;
using Validators;

namespace Api;

public record ClienteDto(string? Nome, string? Email, string? Senha);

// CRUD de clientes (usado pelo gestor). O POST também serve para o cliente criar a própria conta
// na tela de entrada.
//   GET    /api/clientes        -> lista (com a quantidade de pedidos de cada um)
//   POST   /api/clientes        -> cria
//   PUT    /api/clientes/{id}   -> edita (senha em branco = mantém a atual)
//   DELETE /api/clientes/{id}   -> exclui (só se não tiver pedidos)
public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this IEndpointRouteBuilder app)
    {
        var rotas = app.MapGroup("/api/clientes");

        // busca todos os clientes cadastrados no banco
        rotas.MapGet("/", async (AppDbContext db) =>
        {
        var lista = await db.Usuarios
        .Where(u => u.Tipo == "cliente" || u.Tipo == "Cliente")
        .Select(c => new
        {
            c.Id,
            c.Nome,
            c.Email,
            Pedidos = db.Pedidos.Count(p => p.ClienteId == c.Id)
        })
        .ToListAsync();

        return Results.Ok(lista);
        });


        // cria um novo cliente (ou o próprio cliente se cadastrando)
        rotas.MapPost("/", async (ClienteDto dto, AppDbContext db, HistoricoService historico, HttpContext ctx) =>
        {
            var v = Validar(dto, exigirSenha: true);
            if (v.Erro != null) return Resposta.Erro(v.Erro);

            if (await db.Usuarios.AnyAsync(u => u.Email == v.Email))
                return Resposta.Erro("Já existe uma conta com este e-mail.", 409);

            // Guarda o HASH da senha, nunca a senha digitada.
            var cliente = new Usuario(v.Nome, v.Email, SenhaHasher.Gerar(dto.Senha!));
            db.Usuarios.Add(cliente);
            await db.SaveChangesAsync();

            // Quem fez: o gestor logado (se for ele cadastrando) ou o próprio cliente (cadastro na tela de entrada).
            await historico.Registrar(ctx, EntidadeHistorico.Usuario, cliente.Id, AcaoHistorico.Criou,
                $"Conta do cliente \"{cliente.Nome}\" criada.", autorId: UsuarioAtual.Id(ctx) ?? cliente.Id);

            return Results.Ok(new { cliente.Id, cliente.Nome, cliente.Email, tipo = "cliente" });
        });

        // edita um cliente (senha em branco = mantém a atual)
        rotas.MapPut("/{id:int}", async (int id, ClienteDto dto, AppDbContext db, HistoricoService historico, HttpContext ctx) =>
        {
            var cliente = await db.Usuarios.OfType<Usuario>().FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return Resposta.NaoEncontrado("Cliente não encontrado.");

            var v = Validar(dto, exigirSenha: false);
            if (v.Erro != null) return Resposta.Erro(v.Erro);

            // O e-mail pode ser o mesmo do próprio cliente, mas não de outra pessoa.
            if (await db.Usuarios.AnyAsync(u => u.Email == v.Email && u.Id != id))
                return Resposta.Erro("Já existe uma conta com este e-mail.", 409);

            cliente.Nome = v.Nome;
            cliente.Email = v.Email;
            if (!string.IsNullOrEmpty(dto.Senha))
                cliente.Senha = SenhaHasher.Gerar(dto.Senha);

            await db.SaveChangesAsync();
            await historico.Registrar(ctx, EntidadeHistorico.Usuario, cliente.Id, AcaoHistorico.Editou,
                $"Cliente \"{cliente.Nome}\" editado" + (string.IsNullOrEmpty(dto.Senha) ? "." : " (senha alterada)."));

            return Results.Ok(new { cliente.Id, cliente.Nome, cliente.Email, tipo = "cliente" });
        });

        rotas.MapDelete("/{id:int}", async (int id, AppDbContext db, HistoricoService historico, HttpContext ctx) =>
        {
            var cliente = await db.Usuarios.OfType<Usuario>().FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return Resposta.NaoEncontrado("Cliente não encontrado.");

            if (await db.Pedidos.AnyAsync(p => p.ClienteId == id))
                return Resposta.Erro("Este cliente tem pedidos registrados e não pode ser excluído.", 409);

            db.Usuarios.Remove(cliente);
            await db.SaveChangesAsync();
            await historico.Registrar(ctx, EntidadeHistorico.Usuario, id, AcaoHistorico.Excluiu,
                $"Cliente \"{cliente.Nome}\" excluído.");

            return Results.NoContent();
        });
    }

    // Reaproveita o UsuarioValidator de vocês (via ValidadorDeUsuario).
    // Devolve uma "tupla" com três valores: a mensagem de erro (ou null), o nome e o e-mail já tratados.
    private static (string? Erro, string Nome, string Email) Validar(ClienteDto d, bool exigirSenha)
    {
        // ValidarNome tira espaços repetidos e põe a primeira letra maiúscula (guarda em validador.Nome).
        if (!UsuarioValidator.ValidarNome(d.Nome ?? "", out string nomeTratado))
            return ("Informe o nome.", "", "");


        string email = (d.Email ?? "").Replace(" ", "").ToLower();
        bool emailOk = UsuarioValidator.ValidarEmail(email) && MailAddress.TryCreate(email, out _);
        if (!emailOk)
            return ("Use um e-mail @gmail.com, @hotmail.com, @outlook.com ou @yahoo.com.", "", "");

        // Na edição, senha em branco significa "não quero trocar".
        if (exigirSenha || !string.IsNullOrEmpty(d.Senha))
        {
            if (!UsuarioValidator.ValidarSenha(d.Senha ?? ""))
                return ("A senha precisa ter de 6 a 10 caracteres.", "", "");
        }

        return (null, nomeTratado, email);
    }
}
