using Loja.Application.Common;
using Loja.Application.Pedidos;

namespace Loja.Api.Endpoints;

/// <summary>
/// Endpoints HTTP de pedidos (Minimal API).
///
/// A API é uma camada FINA: recebe HTTP -> chama o caso de uso -> devolve HTTP. Nenhuma regra de negócio aqui.
/// Se você se pegar escrevendo um "if" de regra de negócio neste arquivo, ele provavelmente pertence ao Domain.
///
/// POR QUE Minimal API (e não Controllers)?
/// - Menos cerimônia para uma API simples, e cada endpoint recebe SÓ o que usa (o caso de uso específico) em vez de
///   um controller que injeta 10 dependências, das quais cada ação usa 1 (isso feriria o ISP/SRP).
///   Controllers também seriam uma escolha válida: a arquitetura não depende dessa decisão.
///
/// POR QUE o caso de uso chega como parâmetro?
/// - O ASP.NET injeta <c>ICasoDeUso&lt;,&gt;</c> (a abstração). Já vem decorado com log e tratamento de exceções.
/// </summary>
public static class PedidosEndpoints
{
    public static IEndpointRouteBuilder MapPedidos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/pedidos").WithTags("Pedidos");

        grupo.MapPost("/", CriarAsync);
        grupo.MapGet("/", ListarAsync);
        grupo.MapGet("/{id:guid}", ObterAsync).WithName("ObterPedido");
        grupo.MapPost("/{id:guid}/confirmar", ConfirmarAsync);

        return app;
    }

    private static async Task<IResult> CriarAsync(
        CriarPedidoRequest request,
        ICasoDeUso<CriarPedidoRequest, PedidoResponse> casoDeUso,
        CancellationToken cancellationToken)
    {
        var resultado = await casoDeUso.ExecutarAsync(request, cancellationToken);

        // 201 Created + cabeçalho Location apontando para o novo recurso (convenção REST).
        return resultado.ParaHttp(pedido => Results.CreatedAtRoute("ObterPedido", new { id = pedido.Id }, pedido));
    }

    private static async Task<IResult> ListarAsync(
        ICasoDeUso<ListarPedidosRequest, IReadOnlyList<PedidoResponse>> casoDeUso,
        CancellationToken cancellationToken) =>
        (await casoDeUso.ExecutarAsync(new ListarPedidosRequest(), cancellationToken)).ParaHttp();

    private static async Task<IResult> ObterAsync(
        Guid id,
        ICasoDeUso<ObterPedidoRequest, PedidoResponse> casoDeUso,
        CancellationToken cancellationToken) =>
        (await casoDeUso.ExecutarAsync(new ObterPedidoRequest(id), cancellationToken)).ParaHttp();

    private static async Task<IResult> ConfirmarAsync(
        Guid id,
        ICasoDeUso<ConfirmarPedidoRequest, PedidoResponse> casoDeUso,
        CancellationToken cancellationToken) =>
        (await casoDeUso.ExecutarAsync(new ConfirmarPedidoRequest(id), cancellationToken)).ParaHttp();
}
