using PI_1ANO.Data​;
using Models;
using Microsoft.EntityFrameworkCore;
using Validators;
using Services;

Cliente cliente = new Cliente(
    nome: "Josué",
    email: "josue@example.com",
    senha: "senha123!"
);

UsuarioValidator validator = new ClienteValidator();

UsuarioService usuarioService = new UsuarioService(validator);

await usuarioService.CadastrarUsuario(cliente);