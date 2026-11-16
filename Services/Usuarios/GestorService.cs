using Models;
using PI_1ANO.Data;
using Validators;


namespace Services;

public class GestorService : UsuarioService
{
    public GestorService(UsuarioValidator validator) : base(validator)
    {
    }
    private readonly UsuarioValidator _validator;

    

}