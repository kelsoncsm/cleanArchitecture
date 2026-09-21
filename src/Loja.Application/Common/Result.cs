namespace Loja.Application.Common;

/// <summary>Categoria do erro. A API traduz cada categoria para um status HTTP diferente (400, 404, 422...).</summary>
public enum TipoErro
{
    /// <summary>A requisição está malformada (ex.: lista de itens ausente).</summary>
    Validacao = 1,

    /// <summary>O recurso pedido não existe.</summary>
    NaoEncontrado = 2,

    /// <summary>A requisição é válida, mas viola uma regra de negócio (ex.: confirmar pedido vazio).</summary>
    RegraDeNegocio = 3
}

public sealed record Erro(TipoErro Tipo, string Mensagem);

/// <summary>
/// PADRÃO RESULT: o retorno diz explicitamente "deu certo, aqui está o valor" OU "falhou, aqui está o erro".
///
/// POR QUE não lançar exceções para tudo?
/// - "Pedido não encontrado" ou "regra de negócio violada" são resultados ESPERADOS do fluxo, não situações
///   excepcionais. Exceções são lentas, escondem o fluxo (viram um "goto" invisível) e não aparecem na assinatura
///   do método. Com Result, o compilador e o leitor VEEM que aquele método pode falhar.
/// - Exceções ficam reservadas para o inesperado de verdade (banco caiu, bug), tratado por um handler global.
///
/// O construtor é privado: só é possível criar via <see cref="Sucesso"/> ou <see cref="Falha"/>,
/// então nunca existe um Result "meio sucesso, meio erro".
/// </summary>
public sealed class Result<T>
{
    private readonly T? _valor;
    private readonly Erro? _erro;

    public bool EhSucesso { get; }

    public T Valor => EhSucesso
        ? _valor!
        : throw new InvalidOperationException("Não é possível ler o valor de um resultado de falha.");

    public Erro Erro => !EhSucesso
        ? _erro!
        : throw new InvalidOperationException("Não é possível ler o erro de um resultado de sucesso.");

    private Result(T? valor, Erro? erro, bool ehSucesso)
    {
        _valor = valor;
        _erro = erro;
        EhSucesso = ehSucesso;
    }

    public static Result<T> Sucesso(T valor) => new(valor, null, true);

    public static Result<T> Falha(TipoErro tipo, string mensagem) => new(default, new Erro(tipo, mensagem), false);
}
