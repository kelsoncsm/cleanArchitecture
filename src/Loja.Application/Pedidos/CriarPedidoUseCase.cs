using Loja.Application.Common;
using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Application.Pedidos;

/// <summary>
/// Caso de uso: criar um pedido.
///
/// O QUE a camada Application faz: ORQUESTRA. Ela não contém regra de negócio; apenas coordena
/// "receber dados -> pedir ao Domain que crie/valide -> persistir -> devolver resposta".
///
/// POR QUE não há validação de nome vazio, preço negativo etc. aqui?
/// - Essas regras já existem no Domain (Pedido/ItemPedido/Dinheiro). Repeti-las aqui violaria o DRY e
///   criaria duas "fontes da verdade" que podem divergir. Se o Domain rejeitar, lança <c>DominioException</c>,
///   e o decorator de exceções a converte em Result de falha.
/// - Aqui validamos só o que é do FORMATO da requisição (a lista de itens veio?).
///
/// POR QUE <c>TimeProvider</c> em vez de <c>DateTime.UtcNow</c>?
/// - Dependemos de uma abstração do relógio (DIP). Nos testes injetamos um relógio fixo e o resultado fica
///   determinístico, sem "flaky tests".
/// </summary>
public sealed class CriarPedidoUseCase : ICasoDeUso<CriarPedidoRequest, PedidoResponse>
{
    private readonly IPedidoRepository _repository;
    private readonly TimeProvider _relogio;

    public CriarPedidoUseCase(IPedidoRepository repository, TimeProvider relogio)
    {
        _repository = repository;
        _relogio = relogio;
    }

    public async Task<Result<PedidoResponse>> ExecutarAsync(CriarPedidoRequest request, CancellationToken cancellationToken)
    {
        // Guard clause: falha rápido e evita aninhar o "caminho feliz" dentro de vários ifs (código mais legível).
        if (request.Itens is null || request.Itens.Count == 0)
            return Result<PedidoResponse>.Falha(TipoErro.Validacao, "Informe ao menos um item no pedido.");

        var pedido = Pedido.Criar(request.Cliente, request.ClienteVip, _relogio.GetUtcNow().UtcDateTime);

        foreach (var item in request.Itens)
            pedido.AdicionarItem(item.Produto, Dinheiro.De(item.PrecoUnitario), item.Quantidade);

        await _repository.AdicionarAsync(pedido, cancellationToken);

        return Result<PedidoResponse>.Sucesso(pedido.ParaResponse());
    }
}
