using Loja.Application.Common;
using Loja.Domain.Common;

namespace Loja.Application.Decorators;

/// <summary>
/// PADRÃO DECORATOR: "embrulha" um caso de uso e acrescenta um comportamento SEM alterá-lo.
///
/// Este decorator converte <see cref="DominioException"/> (regra de negócio violada) em um
/// <c>Result</c> de falha do tipo RegraDeNegocio.
///
/// POR QUE assim e não um try/catch dentro de cada caso de uso?
/// - DRY: escrito uma vez, vale para TODOS os casos de uso, atuais e futuros.
/// - SRP: o caso de uso fica só com o caminho feliz; tratar exceção é OUTRA responsabilidade.
/// - OCP: adicionamos o comportamento sem editar as classes existentes.
/// - Exceções que NÃO são de domínio (banco caiu, bug) continuam subindo, para o handler global da API
///   registrar e devolver 500. Só tratamos o que sabemos tratar.
/// </summary>
public sealed class ExcecaoDeDominioDecorator<TRequest, TResponse> : ICasoDeUso<TRequest, TResponse>
{
    private readonly ICasoDeUso<TRequest, TResponse> _interno;

    public ExcecaoDeDominioDecorator(ICasoDeUso<TRequest, TResponse> interno) => _interno = interno;

    public async Task<Result<TResponse>> ExecutarAsync(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _interno.ExecutarAsync(request, cancellationToken);
        }
        catch (DominioException ex)
        {
            return Result<TResponse>.Falha(TipoErro.RegraDeNegocio, ex.Message);
        }
    }
}
