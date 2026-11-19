using System.Security.Cryptography;

namespace Services;

public static class SenhaHasher
{
    private const int Iteracoes = 100_000; // quanto mais, mais difícil de quebrar por força bruta

    // Transforma "abc123" em algo como "Sg3k...==:Zm9v...==" (sal : hash) para guardar no banco.
    public static string Gerar(string senha)
    {
        // O "sal" é um valor aleatório. Ele faz duas pessoas com a mesma senha
        // terem hashes diferentes no banco.
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(sal) + ":" + Convert.ToBase64String(hash);
    }

    // Confere se a senha digitada no login corresponde ao que está guardado.
    public static bool Conferir(string senha, string armazenado)
    {
        string[] partes = armazenado.Split(':');
        if (partes.Length != 2) return false;

        try
        {
            byte[] sal = Convert.FromBase64String(partes[0]);
            byte[] esperado = Convert.FromBase64String(partes[1]);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(senha, sal, Iteracoes, HashAlgorithmName.SHA256, esperado.Length);

            // Compara em tempo constante (não revela, pelo tempo de resposta, onde errou).
            return CryptographicOperations.FixedTimeEquals(hash, esperado);
        }
        catch (FormatException)
        {
            return false; // valor guardado no banco não está no formato esperado
        }
    }
}
