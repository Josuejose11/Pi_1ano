﻿using Api;
using Models;
using PI_1ANO.Data;
using Services;
using Validators;

var builder = WebApplication.CreateBuilder(args);

// Endereço em que o sistema vai rodar. É só abrir http://localhost:5000 no navegador.
builder.WebHost.UseUrls("http://localhost:5000");

// Registra o AppDbContext no "container de serviços". Assim, cada endpoint pode
// simplesmente pedir um "AppDbContext db" como parâmetro e o ASP.NET entrega um pronto
// (um novo para cada requisição, descartado no final).
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<ProdutoService>();

var app = builder.Build();

// Ao iniciar: aplica as migrations no MySQL e cria o gestor inicial (veja Data/DbInicial.cs).
using (var escopo = app.Services.CreateScope())
{
    var db = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInicial.PrepararAsync(db);
}

// "Rede de proteção": se algum endpoint der erro, em vez de o navegador receber uma página
// de erro do servidor, ele recebe {"erro": "mensagem"}. O HTML já sabe mostrar essa mensagem.
app.Use(async (contexto, proximo) =>
{
    try
    {
        await proximo();
    }
    catch (BadHttpRequestException)
    {
        // JSON malformado ou com tipo errado (ex.: texto onde devia ser número)
        contexto.Response.StatusCode = 400;
        await contexto.Response.WriteAsJsonAsync(new { erro = "Os dados enviados estão incompletos ou em formato inválido." });
    }
    catch (Exception ex)
    {
        contexto.Response.StatusCode = 500;
        await contexto.Response.WriteAsJsonAsync(new { erro = "Erro no servidor: " + ex.Message });
    }
});

// Entrega o index.html da pasta wwwroot quando alguém abre http://localhost:5000
app.UseDefaultFiles();
app.UseStaticFiles();

// O HTML chama /api/ping para descobrir se o servidor C# está no ar.
// Se responder, ele usa o banco de verdade; se não, abre em "modo demonstração".
app.MapGet("/api/ping", () => Results.Ok(new { ok = true }));

// Cada linha abaixo registra um grupo de rotas (cada um está em um arquivo da pasta Api).
app.MapLoginEndpoints();
app.MapProdutoEndpoints();
app.MapClienteEndpoints();
app.MapPedidoEndpoints();

app.Run();
