using Models;
using PI_1ANO.Data;
namespace Validators;

public class ProdutoValidator
{
    // VALIDACAO TOTAL DO USUARIO
    public bool ValidarProduto(Produto produto)
    {
        if (!ValidarNome(produto.Nome))
        {
            Console.WriteLine("Nome inválido");
            return false;
        }
        if (!ValidarPreco(produto.Preco))
        {
            Console.WriteLine("Preço inválido");
            return false;
        }
        if (!ValidarEstoque(produto.Estoque))
        {
            Console.WriteLine("Estoque inválido");
            return false;
        }

        return true;
    }

    // Validacao nome
    public bool ValidarNome(string nome)
    {
        if (!nome.Any(char.IsLetter))                            
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(nome.Trim()))                            
        {
            return false;
        }
        nome = string.Join(" ", nome.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        nome = char.ToUpper(nome[0]) + nome[1..];
        return true;
    }

    // Validacao preço
    public bool ValidarPreco(decimal preco)
    {
        if (decimal.Round(preco, 2) != preco)
        {
            return false;
        }
        if (preco <= 0)
        {
            return false;
        }
        return true;
    }

    // Validacao preço
    public bool ValidarEstoque(int estoque)
    {
        if (estoque <= 0)
        {
            return false;
        }
        return true;
    }
}