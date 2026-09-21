using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Domain.Descontos;

/// <summary>Estratégia concreta: clientes VIP ganham 10% sobre o subtotal.</summary>
/// <remarks>
/// O percentual é uma constante nomeada (e não o "número mágico" 10 espalhado no código):
/// o nome documenta a intenção e há um único lugar para alterar.
/// </remarks>
public sealed class DescontoClienteVip : IPoliticaDesconto
{
    private const decimal PercentualVip = 10m;

    public string Nome => "Cliente VIP";

    public Dinheiro Calcular(Pedido pedido) =>
        pedido.ClienteVip ? pedido.Subtotal.Percentual(PercentualVip) : Dinheiro.Zero;
}
