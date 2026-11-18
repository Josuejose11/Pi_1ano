namespace Api;

// Ajudante para devolver erros sempre no mesmo formato que o HTML espera: {"erro": "mensagem"}.
// O status HTTP diz o tipo do problema:
//   400 = dado inválido, 401 = login incorreto, 404 = não existe, 409 = conflito (ex.: e-mail repetido).
public static class Resposta
{
    public static IResult Erro(string mensagem, int status = 400)
        => Results.Json(new { erro = mensagem }, statusCode: status);

    public static IResult NaoEncontrado(string mensagem)
        => Erro(mensagem, 404);
}
