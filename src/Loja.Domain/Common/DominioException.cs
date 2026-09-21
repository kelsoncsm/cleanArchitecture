namespace Loja.Domain.Common;

/// <summary>
/// Exceção lançada quando uma REGRA DE NEGÓCIO (invariante) é violada.
///
/// POR QUE existe uma exceção própria e não <see cref="InvalidOperationException"/>?
/// - Permite que as camadas externas diferenciem "o usuário tentou algo que o negócio não permite"
///   (ex.: confirmar pedido vazio) de "bug/erro de infraestrutura" (ex.: banco fora do ar).
/// - O nome fala a linguagem do negócio (Ubiquitous Language do DDD).
///
/// A Application captura esta exceção e a transforma em um <c>Result</c> de falha, então ela
/// nunca "vaza" como erro 500 para o cliente.
/// </summary>
public sealed class DominioException : Exception
{
    public DominioException(string mensagem) : base(mensagem)
    {
    }
}
