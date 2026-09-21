using Loja.Application.Common;
using Loja.Application.Decorators;
using Loja.Application.Pedidos;
using Loja.Domain.Descontos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Loja.Application;

/// <summary>
/// Cada camada expõe UM método de extensão que registra os próprios serviços (<c>AddApplication</c>,
/// <c>AddInfrastructure</c>).
///
/// POR QUE?
/// - Encapsulamento: o Program.cs não precisa saber QUAIS classes existem dentro de cada camada; se criarmos um
///   novo caso de uso, só esta camada muda.
/// - Deixa o Composition Root curto e legível: <c>services.AddApplication().AddInfrastructure()</c>.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Strategy: cada política é registrada contra a MESMA interface. O contêiner entrega todas juntas
        // (IEnumerable<IPoliticaDesconto>) para a CalculadoraDeDesconto. Nova promoção = nova linha aqui (OCP).
        services.AddSingleton<IPoliticaDesconto, DescontoClienteVip>();
        services.AddSingleton<IPoliticaDesconto, DescontoPorQuantidade>();
        services.AddSingleton<CalculadoraDeDesconto>();

        services
            .AddCasoDeUso<CriarPedidoRequest, PedidoResponse, CriarPedidoUseCase>()
            .AddCasoDeUso<ConfirmarPedidoRequest, PedidoResponse, ConfirmarPedidoUseCase>()
            .AddCasoDeUso<ObterPedidoRequest, PedidoResponse, ObterPedidoUseCase>()
            .AddCasoDeUso<ListarPedidosRequest, IReadOnlyList<PedidoResponse>, ListarPedidosUseCase>();

        return services;
    }

    /// <summary>
    /// Registra um caso de uso JÁ "embrulhado" pelos decorators. Quem pedir <c>ICasoDeUso&lt;TReq,TRes&gt;</c>
    /// recebe: Log( ExcecaoDeDominio( CasoDeUso ) ).
    ///
    /// A ordem importa: o Log fica por FORA, então ele enxerga o Result final (inclusive as falhas que o
    /// decorator de exceção converteu).
    /// </summary>
    private static IServiceCollection AddCasoDeUso<TRequest, TResponse, TImplementacao>(this IServiceCollection services)
        where TImplementacao : class, ICasoDeUso<TRequest, TResponse>
    {
        services.AddScoped<TImplementacao>();

        services.AddScoped<ICasoDeUso<TRequest, TResponse>>(provider =>
        {
            ICasoDeUso<TRequest, TResponse> casoDeUso = provider.GetRequiredService<TImplementacao>();

            casoDeUso = new ExcecaoDeDominioDecorator<TRequest, TResponse>(casoDeUso);

            return new LogCasoDeUsoDecorator<TRequest, TResponse>(
                casoDeUso,
                provider.GetRequiredService<ILogger<LogCasoDeUsoDecorator<TRequest, TResponse>>>());
        });

        return services;
    }
}
