using PI_1ANO.Data;
using Models;
using Validators;
using Services;
using Microsoft.EntityFrameworkCore;
Console.Clear();

Console.Write("Nome: ");
string nome = Console.ReadLine() ?? "";

Console.Write("Senha: ");
string senha = Console.ReadLine() ?? "";

Console.Write("Email: ");
string email = Console.ReadLine() ?? "";

// Criar objeto
Usuario usuario = new Cliente(nome, email, senha);

// Validar
UsuarioValidator validator = new UsuarioValidator();

if (!validator.Validar(usuario))
{
    Console.WriteLine("Dados inválidos!");
    return;
}

// // Executar serviço
// UsuarioService service = new UsuarioService();

// service.Cadastrar(usuario);

// Console.WriteLine("Usuário cadastrado com sucesso!");