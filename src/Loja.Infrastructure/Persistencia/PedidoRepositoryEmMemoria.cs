using System.Collections.Concurrent;
using Loja.Domain.Pedidos;

namespace Loja.Infrastructure.Persistencia;

/// <summary>
/// Implementação do <see cref="IPedidoRepository"/> que guarda os pedidos em memória.
///
/// POR QUE em memória e não Entity Framework Core já de início?
/// - YAGNI/simplicidade: o objetivo é mostrar a ARQUITETURA sem exigir banco instalado. Para produção, basta
///   criar uma classe <c>PedidoRepositoryEf : IPedidoRepository</c> e trocar UMA linha no
///   <c>AddInfrastructure</c>. Domain, Application e API não mudam nada - essa é a prova de que a
///   Inversão de Dependência funcionou.
/// - LSP: esta classe pode ser substituída por qualquer outra implementação da interface sem que ninguém perceba.
///
/// POR QUE <see cref="ConcurrentDictionary{TKey,TValue}"/>?
/// - A API atende várias requisições em paralelo e este repositório é singleton (os dados precisam
///   sobreviver entre requisições). Um Dictionary comum não é thread-safe e corromperia o estado.
/// </summary>
public sealed class PedidoRepositoryEmMemoria : IPedidoRepository
{
    private readonly ConcurrentDictionary<Guid, Pedido> _pedidos = new();

    public Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        _pedidos[pedido.Id] = pedido;
        return Task.CompletedTask;
    }

    public Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _pedidos.TryGetValue(id, out var pedido);
        return Task.FromResult(pedido);
    }

    public Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Pedido> lista = _pedidos.Values.OrderBy(p => p.CriadoEmUtc).ToList();
        return Task.FromResult(lista);
    }

    public Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        // Como os objetos vivem em memória, a alteração já está refletida. Com um banco real, aqui iria o
        // SaveChanges/UPDATE. Mantemos o método no contrato para o Application não depender desse detalhe.
        _pedidos[pedido.Id] = pedido;
        return Task.CompletedTask;
    }
}
