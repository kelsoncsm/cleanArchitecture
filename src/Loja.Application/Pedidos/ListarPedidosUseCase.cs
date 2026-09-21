using Loja.Application.Common;
using Loja.Domain.Pedidos;

namespace Loja.Application.Pedidos;

/// <summary>
/// Caso de uso de LEITURA: listar todos os pedidos.
/// (Em produção acrescentaríamos paginação e filtros; aqui mantemos simples de propósito - YAGNI:
/// "You Aren't Gonna Need It", não construa hoje o que ainda não é necessário.)
/// </summary>
public sealed class ListarPedidosUseCase : ICasoDeUso<ListarPedidosRequest, IReadOnlyList<PedidoResponse>>
{
    private readonly IPedidoRepository _repository;

    public ListarPedidosUseCase(IPedidoRepository repository) => _repository = repository;

    public async Task<Result<IReadOnlyList<PedidoResponse>>> ExecutarAsync(ListarPedidosRequest request, CancellationToken cancellationToken)
    {
        var pedidos = await _repository.ListarAsync(cancellationToken);

        IReadOnlyList<PedidoResponse> resposta = pedidos.Select(p => p.ParaResponse()).ToList();
        return Result<IReadOnlyList<PedidoResponse>>.Sucesso(resposta);
    }
}
