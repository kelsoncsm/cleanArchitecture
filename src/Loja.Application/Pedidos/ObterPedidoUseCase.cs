using Loja.Application.Common;
using Loja.Domain.Pedidos;

namespace Loja.Application.Pedidos;

/// <summary>
/// Caso de uso de LEITURA: obter um pedido pelo Id.
///
/// POR QUE separar leitura (queries) de escrita (commands) em classes diferentes? (ideia do CQRS)
/// - Leituras e escritas evoluem em ritmos diferentes: leituras costumam ganhar cache, projeções e paginação;
///   escritas ganham regras e validações. Separadas, uma não atrapalha a outra.
/// </summary>
public sealed class ObterPedidoUseCase : ICasoDeUso<ObterPedidoRequest, PedidoResponse>
{
    private readonly IPedidoRepository _repository;

    public ObterPedidoUseCase(IPedidoRepository repository) => _repository = repository;

    public async Task<Result<PedidoResponse>> ExecutarAsync(ObterPedidoRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _repository.ObterPorIdAsync(request.PedidoId, cancellationToken);

        return pedido is null
            ? Result<PedidoResponse>.Falha(TipoErro.NaoEncontrado, $"Pedido {request.PedidoId} não encontrado.")
            : Result<PedidoResponse>.Sucesso(pedido.ParaResponse());
    }
}
