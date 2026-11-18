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

        rotas.MapGet("/", async (AppDbContext db) =>
        {
            // OfType<Cliente>() filtra a tabela Usuarios e deixa só os clientes (sem gestores).
            var lista = await db.Usuarios
                .OfType<Cliente>()
                .OrderBy(c => c.Nome)
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

        rotas.MapPost("/", async (ClienteDto dto, AppDbContext db) =>
        {
            var v = Validar(dto, exigirSenha: true);
            if (v.Erro != null) return Resposta.Erro(v.Erro);

            if (await db.Usuarios.AnyAsync(u => u.Email == v.Email))
                return Resposta.Erro("Já existe uma conta com este e-mail.", 409);

            // Guarda o HASH da senha, nunca a senha digitada.
            var cliente = new Cliente(v.Nome, v.Email, SenhaHasher.Gerar(dto.Senha!));
            db.Usuarios.Add(cliente);
            await db.SaveChangesAsync();

            return Results.Ok(new { cliente.Id, cliente.Nome, cliente.Email, tipo = "cliente" });
        });

        rotas.MapPut("/{id:int}", async (int id, ClienteDto dto, AppDbContext db) =>
        {
            var cliente = await db.Usuarios.OfType<Cliente>().FirstOrDefaultAsync(c => c.Id == id);
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
            return Results.Ok(new { cliente.Id, cliente.Nome, cliente.Email, tipo = "cliente" });
        });

        rotas.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var cliente = await db.Usuarios.OfType<Cliente>().FirstOrDefaultAsync(c => c.Id == id);
            if (cliente == null) return Resposta.NaoEncontrado("Cliente não encontrado.");

            if (await db.Pedidos.AnyAsync(p => p.ClienteId == id))
                return Resposta.Erro("Este cliente tem pedidos registrados e não pode ser excluído.", 409);

            db.Usuarios.Remove(cliente);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    // Reaproveita o UsuarioValidator de vocês (via ValidadorDeUsuario).
    // Devolve uma "tupla" com três valores: a mensagem de erro (ou null), o nome e o e-mail já tratados.
    private static (string? Erro, string Nome, string Email) Validar(ClienteDto d, bool exigirSenha)
    {
        var validador = new ValidadorDeUsuario();

        // ValidarNome tira espaços repetidos e põe a primeira letra maiúscula (guarda em validador.Nome).
        if (!validador.ValidarNome(d.Nome ?? ""))
            return ("Informe o nome.", "", "");

        string email = (d.Email ?? "").Replace(" ", "").ToLower();
        bool emailOk = validador.ValidarEmail(email) && MailAddress.TryCreate(email, out _);
        if (!emailOk)
            return ("Use um e-mail @gmail.com, @hotmail.com, @outlook.com ou @yahoo.com.", "", "");

        // Na edição, senha em branco significa "não quero trocar".
        if (exigirSenha || !string.IsNullOrEmpty(d.Senha))
        {
            if (!validador.ValidarSenha(d.Senha ?? ""))
                return ("A senha precisa ter de 6 a 10 caracteres.", "", "");
        }

        return (null, validador.Nome, validador.Email);
    }
}
