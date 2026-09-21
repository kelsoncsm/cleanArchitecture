using Loja.Application.Common;

namespace Loja.Api.Endpoints;

/// <summary>
/// Traduz o <see cref="Result{T}"/> da Application para uma resposta HTTP.
///
/// POR QUE esta tradução mora na API e não na Application?
/// - "HTTP 404" é um conceito de TRANSPORTE. A Application não pode saber que existe HTTP (amanhã o mesmo caso de uso
///   pode ser chamado por uma fila ou por um console). Cada porta de entrada traduz o Result do seu jeito.
/// - Centralizado aqui, todos os endpoints respondem erros de forma consistente (DRY).
///
/// Usamos <c>ProblemDetails</c> (RFC 7807), o formato padrão da indústria para erros em APIs HTTP.
/// </summary>
public static class ResultHttpExtensions
{
    public static IResult ParaHttp<T>(this Result<T> resultado, Func<T, IResult>? aoSucesso = null)
    {
        if (resultado.EhSucesso)
            return aoSucesso is null ? Results.Ok(resultado.Valor) : aoSucesso(resultado.Valor);

        var erro = resultado.Erro;

        return erro.Tipo switch
        {
            TipoErro.NaoEncontrado => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso não encontrado", detail: erro.Mensagem),
            TipoErro.Validacao => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Requisição inválida", detail: erro.Mensagem),
            TipoErro.RegraDeNegocio => Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Regra de negócio violada", detail: erro.Mensagem),
            _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Erro inesperado")
        };
    }
}
