using Models;
using Validators;
using Microsoft.EntityFrameworkCore;
namespace Services;

public class UsuarioService
{
    private readonly UsuarioValidator _validator;

    public UsuarioService(UsuarioValidator validator)
    {
        _validator = validator;
    }
    

    public bool CadastrarUsuario(Usuario usuario)
    {
        if (!_validator.ValidarNome(usuario.Nome))
            return false;

        if (!_validator.ValidarEmail(usuario.Email))
            return false;

        if (!_validator.ValidarSenha(usuario.Senha))
            return false;

        // Aqui entraria a persistência no banco de dados.

        return true;
    }
}