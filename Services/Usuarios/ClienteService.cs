using Models;
using PI_1ANO.Data;
using Validators;


namespace Services;

public class ClienteService : UsuarioService
{
    public ClienteService(UsuarioValidator validator) : base(validator)
    {
    }
    private readonly UsuarioValidator _validator;

    
}