using System.Diagnostics;
using Loja.Application.Common;
using Microsoft.Extensions.Logging;

namespace Loja.Application.Decorators;

/// <summary>
/// DECORATOR de observabilidade: registra o nome do caso de uso, o resultado e o tempo de execução.
///
/// POR QUE é uma preocupação transversal (cross-cutting concern) tratada com decorator?
/// - Log em todo caso de uso, copiado à mão, poluiria o código de negócio e seria esquecido em algum. Aqui ele
///   é aplicado automaticamente a todos, em um único arquivo.
///
/// POR QUE NÃO logamos o conteúdo do request?
/// - Requests podem conter dados pessoais (nome do cliente, etc.). Logar só o NOME do caso de uso e o resultado
///   evita vazar dados sensíveis para arquivos de log (boa prática de segurança / LGPD).
/// </summary>
public sealed class LogCasoDeUsoDecorator<TRequest, TResponse> : ICasoDeUso<TRequest, TResponse>
{
    private readonly ICasoDeUso<TRequest, TResponse> _interno;
    private readonly ILogger<LogCasoDeUsoDecorator<TRequest, TResponse>> _logger;

    public LogCasoDeUsoDecorator(
        ICasoDeUso<TRequest, TResponse> interno,
        ILogger<LogCasoDeUsoDecorator<TRequest, TResponse>> logger)
    {
        _interno = interno;
        _logger = logger;
    }

    public async Task<Result<TResponse>> ExecutarAsync(TRequest request, CancellationToken cancellationToken)
    {
        var nome = typeof(TRequest).Name;
        var cronometro = Stopwatch.StartNew();

        var resultado = await _interno.ExecutarAsync(request, cancellationToken);

        cronometro.Stop();

        if (resultado.EhSucesso)
            _logger.LogInformation("Caso de uso {CasoDeUso} concluído em {Ms} ms.", nome, cronometro.ElapsedMilliseconds);
        else
            _logger.LogWarning(
                "Caso de uso {CasoDeUso} falhou ({TipoErro}): {Mensagem} [{Ms} ms].",
                nome, resultado.Erro.Tipo, resultado.Erro.Mensagem, cronometro.ElapsedMilliseconds);

        return resultado;
    }
}
