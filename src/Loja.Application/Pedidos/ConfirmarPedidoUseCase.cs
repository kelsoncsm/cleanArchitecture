using Loja.Application.Common;
using Loja.Domain.Descontos;
using Loja.Domain.Pedidos;

namespace Loja.Application.Pedidos;

/// <summary>
/// Caso de uso: confirmar um pedido, aplicando o melhor desconto disponível.
///
/// Repare como o fluxo se lê como uma receita (Clean Code: funções que fazem UMA coisa, no mesmo nível de abstração):
///   1. buscar  2. calcular desconto (Strategy)  3. confirmar (regra no Domain)  4. salvar  5. responder.
///
/// As dependências chegam pelo CONSTRUTOR (Injeção de Dependência) e são todas abstrações ou serviços de domínio,
/// então testamos este caso de uso sem banco de dados e sem ASP.NET.
/// </summary>
public sealed class ConfirmarPedidoUseCase : ICasoDeUso<ConfirmarPedidoRequest, PedidoResponse>
{
    private readonly IPedidoRepository _repository;
    private readonly CalculadoraDeDesconto _calculadora;

    public ConfirmarPedidoUseCase(IPedidoRepository repository, CalculadoraDeDesconto calculadora)
    {
        _repository = repository;
        _calculadora = calculadora;
    }

    public async Task<Result<PedidoResponse>> ExecutarAsync(ConfirmarPedidoRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _repository.ObterPorIdAsync(request.PedidoId, cancellationToken);
        if (pedido is null)
            return Result<PedidoResponse>.Falha(TipoErro.NaoEncontrado, $"Pedido {request.PedidoId} não encontrado.");

        var desconto = _calculadora.CalcularMelhorDesconto(pedido);
        pedido.Confirmar(desconto);

        await _repository.AtualizarAsync(pedido, cancellationToken);

        return Result<PedidoResponse>.Sucesso(pedido.ParaResponse());
    }
}
