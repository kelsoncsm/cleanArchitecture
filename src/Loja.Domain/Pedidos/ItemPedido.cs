using Loja.Domain.Common;

namespace Loja.Domain.Pedidos;

/// <summary>
/// Item de um pedido (produto + preço + quantidade).
///
/// POR QUE o construtor é <c>internal</c>?
/// - Um item só faz sentido DENTRO de um pedido. Se qualquer um pudesse criar e alterar itens soltos,
///   o pedido perderia o controle do próprio total. Quem manda no item é o <see cref="Pedido"/>
///   (esse é o conceito de "Aggregate Root" do DDD: um único ponto de entrada que protege as regras).
///
/// POR QUE as validações estão aqui e não no controller/use case?
/// - Regra de negócio pertence ao Domain. Assim, ela vale para QUALQUER porta de entrada
///   (API, fila, job, teste) sem precisar ser duplicada.
/// </summary>
public sealed class ItemPedido
{
    public string Produto { get; }
    public Dinheiro PrecoUnitario { get; }
    public int Quantidade { get; }
    public Dinheiro Subtotal => PrecoUnitario.Multiplicar(Quantidade);

    internal ItemPedido(string produto, Dinheiro precoUnitario, int quantidade)
    {
        if (string.IsNullOrWhiteSpace(produto))
            throw new DominioException("O nome do produto é obrigatório.");

        if (quantidade <= 0)
            throw new DominioException("A quantidade deve ser maior que zero.");

        Produto = produto.Trim();
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }
}
