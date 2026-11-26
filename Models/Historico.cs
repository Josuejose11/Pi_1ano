namespace Models;


public class Historico
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoEntidade { get; set; }
    public int EntidadeId { get; set; }
    public string Acao { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataHora { get; set; }

    public Historico(int usuarioId, string tipoEntidade, int entidadeId, string acao, string? descricao = null)
    {
        UsuarioId = usuarioId;
        TipoEntidade = tipoEntidade;
        EntidadeId = entidadeId;
        Acao = acao;
        Descricao = descricao;
        DataHora = DateTime.Now;
    }


    

}